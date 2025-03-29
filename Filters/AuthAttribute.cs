using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthAttribute : Attribute, IAsyncActionFilter
{
    private readonly IEnumerable<string> _accesses;

    public AuthAttribute(params string[] accesses)
    {
        _accesses = accesses;
    }

    private IEnumerable<string> GetAccesses(Type type)
    {
        var accesses = _accesses.ToList();
        var preType = type;
        while (type is not null)
        {
            if (type.IsAssignableFrom(typeof(CrudController)))
            {
                var entityType = preType.GenericTypeArguments.FirstOrDefault();
                if (entityType is not null)
                {
                    accesses = accesses.ConvertAll(x => x.Replace("[entity]", entityType.Name));
                }

                break;
            }

            preType = type;
            type = type.BaseType;
        }

        return accesses.Select(x => x.ToLower());
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
            var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
            var methodHasAuthAttribute = controllerAuthAttributes.Any(x => Equals(x, this)) && methodAttributes.Any();
            var hasIgnoreAuthIgnoreAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null;
            if (methodHasAuthAttribute || hasIgnoreAuthIgnoreAttribute)
            {
                await next.Invoke();
                return;
            }
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
        
        if (_accesses?.Count() > 0)
        {
            IEnumerable<string> policies;
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                policies = GetAccesses(controllerActionDescriptor.ControllerTypeInfo);
            }
            else
            {
                throw new Exception("ActionDescription is Not ControllerActionDescriptor");
            }

            var authenticationService = sp.GetService<IAuthService>();
            if (user.Username != "admin" && !await authenticationService.HasAccessAsync(user.Id, policies.ToArray()))
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