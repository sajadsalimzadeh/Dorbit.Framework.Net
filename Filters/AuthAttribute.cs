using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;
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
        if (identityRequest.AccessToken.IsNullOrEmpty())
            throw new AuthenticationException("Access token not set");
        identityRequest.CsrfToken = httpRequest.GetCsrfToken();
        if (identityRequest.CsrfToken.IsNullOrEmpty())
            throw new AuthenticationException("Csrf token not set");

        var serviceProvider = context.HttpContext.RequestServices;
        var identityService = serviceProvider.GetService<IIdentityService>();
        var identity = await identityService.ValidateAsync(identityRequest);
        if (identity is null) throw new AuthenticationException();
        await next.Invoke();
    }
}