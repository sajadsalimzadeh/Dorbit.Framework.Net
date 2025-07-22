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
public class AuthAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _access;

    public AuthAttribute(string access = null)
    {
        _access = access;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var access = _access;
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            throw new Exception("Context is not type of ControllerActionDescriptor");

        if (access.IsNotNullOrEmpty())
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
                        access = access.Replace("{type" + index + "}", genericType.Name);
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
            access = string.Empty;
        }

        var identityRequest = new IdentityValidateRequest()
        {
            Access = access
        };

        var httpRequest = context.HttpContext.Request;
        identityRequest.AccessToken = httpRequest.GetAccessToken();
        if (identityRequest.AccessToken.IsNullOrEmpty())
            throw new AuthenticationException("Access token not set");
        identityRequest.CsrfToken = httpRequest.GetCsrfToken();
        if (identityRequest.CsrfToken.IsNullOrEmpty())
            throw new AuthenticationException("Csrf token not set");

        if (context.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            identityRequest.UserAgent = userAgent;

        var serviceProvider = context.HttpContext.RequestServices;
        var identityService = serviceProvider.GetService<IIdentityService>();
        var identity = await identityService.ValidateAsync(identityRequest);
        if (identity is null) throw new AuthenticationException();
        await next.Invoke();
    }
}