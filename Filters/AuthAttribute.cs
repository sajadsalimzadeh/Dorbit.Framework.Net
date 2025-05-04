using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthAttribute(string access = null) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            throw new Exception("Context is not type of ControllerActionDescriptor");

        var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
        var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
        var methodHasAuthAttribute = controllerAuthAttributes.Any(x => Equals(x, this)) && methodAttributes.Any();
        var hasIgnoreAuthIgnoreAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null;
        if (methodHasAuthAttribute || hasIgnoreAuthIgnoreAttribute)
        {
            await next.Invoke();
            return;
        }

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

        var identityRequest = new IdentityValidateRequest()
        {
            Access = access
        };
        var httpRequest = context.HttpContext.Request;

        if (httpRequest.Headers.ContainsKey("Authorization")) identityRequest.AccessToken = httpRequest.Headers["Authorization"];
        else if (httpRequest.Query.ContainsKey("ApiKey")) identityRequest.AccessToken = httpRequest.Query["ApiKey"];
        else if (httpRequest.Cookies.ContainsKey("ApiKey")) identityRequest.AccessToken = httpRequest.Cookies["ApiKey"];

        if (identityRequest.AccessToken.IsNullOrEmpty())
            throw new AuthenticationException("Access token not set");

        identityRequest.AccessToken = identityRequest.AccessToken!.Replace("Bearer ", "");

        if (httpRequest.Cookies.ContainsKey("CsrfToken")) identityRequest.CsrfToken = httpRequest.Cookies["CsrfToken"];

        if (identityRequest.CsrfToken.IsNullOrEmpty())
            throw new AuthenticationException("Csrf token not set");

        if (context.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            identityRequest.UserAgent = userAgent;

        var serviceProvider = context.HttpContext.RequestServices;
        var identityService = serviceProvider.GetService<IIdentityService>();
        if (await identityService.ValidateAsync(identityRequest))
        {
            await next.Invoke();
        }
    }
}