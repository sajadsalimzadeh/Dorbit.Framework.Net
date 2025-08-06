using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Identities;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthAttribute(string access = null) : Attribute, IAsyncActionFilter
{
    public bool IsOptional { get; set; }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tempAccess = access;
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            throw new Exception("Context is not type of ControllerActionDescriptor");

        if (tempAccess.IsNotNullOrEmpty())
        {
            var type = actionDescriptor.ControllerTypeInfo as Type;
            var preType = type;
            while (type is not null)
            {
                if (type.IsAssignableFrom(typeof(CrudController)))
                {
                    var index = 0;
                    foreach (var genericType in preType.GenericTypeArguments)
                    {
                        tempAccess = tempAccess.Replace("{type" + index + "}", genericType.Name);
                        index++;
                    }

                    break;
                }

                preType = type;
                type = type.BaseType;
            }
        }

        var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
        var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
        var methodHasAuthAttribute = controllerAuthAttributes.Any(x => Equals(x, this)) && methodAttributes.Any();
        var hasIgnoreAuthIgnoreAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null;
        if (methodHasAuthAttribute || hasIgnoreAuthIgnoreAttribute)
        {
            tempAccess = string.Empty;
        }

        var identityRequest = new IdentityValidateRequest()
        {
            Access = tempAccess
        };

        identityRequest.IpV4 = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        identityRequest.IpV6 = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString();
        identityRequest.UserAgent = context.HttpContext.Request.Headers.FirstValueOrDefault("User-Agent");
        var httpRequest = context.HttpContext.Request;
        
        identityRequest.AccessToken = httpRequest.GetAccessToken();
        identityRequest.CsrfToken = httpRequest.GetCsrfToken();

        var serviceProvider = context.HttpContext.RequestServices;

        try
        {
            var identityService = serviceProvider.GetService<IIdentityService>();
            var identity = await identityService.ValidateAsync(identityRequest);
            if (identity is null)
                throw new AuthenticationException();


            for (var i = 0; i < context.ActionDescriptor.Parameters.Count; i++)
            {
                var parameter = context.ActionDescriptor.Parameters[i];
                var fromClaimAttribute = parameter.ParameterType.GetCustomAttribute<FromClaimAttribute>();
                if (fromClaimAttribute is null) continue;

                if (!identity.Claims.TryGetValue(fromClaimAttribute.Name, out var claimValue))
                    throw new UnauthorizedAccessException($"claim {fromClaimAttribute.Name} not found");

                if (parameter.ParameterType == typeof(Guid))
                {
                    context.ActionArguments[parameter.Name] = Guid.Parse(claimValue);
                }
                else if (parameter.ParameterType == typeof(int))
                {
                    context.ActionArguments[parameter.Name] = int.Parse(claimValue);
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    context.ActionArguments[parameter.Name] = claimValue;
                }
                else
                {
                    context.ActionArguments[parameter.Name] = JsonSerializer.Deserialize(claimValue, parameter.ParameterType);
                }
            }
        }
        catch
        {
            if (!IsOptional)
            {
                throw;
            }
        }

        await next.Invoke();
    }
}