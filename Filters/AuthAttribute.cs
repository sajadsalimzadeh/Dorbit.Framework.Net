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
public class AuthAttribute(params string[] accesses) : Attribute, IAsyncActionFilter
{
    public string[] Accesses { get; set; } = accesses;
    public bool IsOptional { get; set; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var accesses = Accesses;
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            throw new Exception("Context is not type of ControllerActionDescriptor");

        if (accesses.IsNotNullOrEmpty())
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
                        accesses = accesses.Select(x => x.Replace("{type" + index + "}", genericType.Name)).ToArray();
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
            accesses = [];
        }

        var identityRequest = new IdentityValidateRequest();
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
            
            
            if (accesses is not null && accesses.Length > 0 && !identity.IsFullAccess && !accesses.Any(x => identity.DeepAccessibility.Contains(x)))
                throw new UnauthorizedAccessException();

            for (var i = 0; i < context.ActionDescriptor.Parameters.Count; i++)
            {
                var parameter = context.ActionDescriptor.Parameters[i];
                var fromClaimAttribute = parameter.ParameterType.GetCustomAttribute<FromClaimAttribute>();
                if (fromClaimAttribute is null) continue;

                var claimDto = identity.Claims.FirstOrDefault(x => x.Type.Equals(fromClaimAttribute.Type, StringComparison.CurrentCultureIgnoreCase));
                if (claimDto == null || claimDto.Value.IsNullOrEmpty())
                    throw new UnauthorizedAccessException($"claim {fromClaimAttribute.Type} not found");

                if (parameter.ParameterType == typeof(Guid))
                {
                    context.ActionArguments[parameter.Name] = Guid.Parse(claimDto.Value);
                }
                else if (parameter.ParameterType == typeof(int))
                {
                    context.ActionArguments[parameter.Name] = int.Parse(claimDto.Value);
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    context.ActionArguments[parameter.Name] = claimDto.Value;
                }
                else
                {
                    context.ActionArguments[parameter.Name] = JsonSerializer.Deserialize(claimDto.Value, parameter.ParameterType);
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