using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
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
        //Find Token

        var sp = context.HttpContext.RequestServices;
        var userResolver = sp.GetService<IUserResolver>();

        var user = userResolver.User;
        if (user is null) throw new AuthenticationException();

        if (!user.IsActive) throw new UnauthorizedAccessException("Your user is disabled");

        var userStateService = sp.GetService<IUserStateService>();
        var state = userStateService.GetUserState(user.Id?.ToString());
        state.Url = context.HttpContext.Request.GetDisplayUrl();
        state.LastRequestTime = DateTime.UtcNow;
        if (context.HttpContext.Request.Headers.TryGetValue("User-Agent", out var agent))
        {
            userStateService.LoadClientInfo(state, agent);
        }

        userStateService.LoadGeoInfo(state, context.HttpContext.Connection.RemoteIpAddress?.ToString());

        foreach (var authService in sp.GetServices<IAuthService>())
        {
            if (!await authService.IsTokenValid(context.HttpContext, user.Claims))
            {
                throw new AuthenticationException("InvalidToken");
            }
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


            var authenticationService = sp.GetService<IAuthService>();
            if (user.Username != "admin" && !await authenticationService.HasAccessAsync(user.Id, access))
            {
                throw new UnauthorizedAccessException("AccessDenied");
            }
        }

        await OnActionExecutionAsync(context, user.Claims);

        await next.Invoke();
    }

    protected virtual Task OnActionExecutionAsync(ActionExecutingContext context, ClaimsPrincipal claims)
    {
        return Task.CompletedTask;
    }
}