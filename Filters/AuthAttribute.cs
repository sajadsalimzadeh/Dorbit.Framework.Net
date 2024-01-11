using System.Reflection;
using System.Security.Authentication;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Models.Users;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthAttribute : Attribute, IAsyncAuthorizationFilter
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

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
            var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
            if (controllerAuthAttributes.Any(x => Equals(x, this)) && methodAttributes.Any()) return;
            if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null) return;
        }

        var sp = context.HttpContext.RequestServices;
        var userResolver = sp.GetService<IUserResolver>();

        var user = userResolver.User;
        if (user is null) throw new AuthenticationException();

        var userStateService = sp.GetService<IUserStateService>();
        var state = userStateService.GetUserState(user.Id);
        state.Url = context.HttpContext.Request.GetDisplayUrl();
        state.LastRequestTime = DateTime.UtcNow;
        if (context.HttpContext.Request.Headers.TryGetValue("User-Agent", out var agent))
        {
            userStateService.LoadClientInfo(state, agent);
        }

        userStateService.LoadGeoInfo(state, context.HttpContext.Connection.RemoteIpAddress?.ToString());
        if (_accesses?.Count() > 0)
        {
            IEnumerable<string> policies;
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                policies = GetAccesses(controllerActionDescriptor.ControllerTypeInfo);
            }
            else throw new Exception("ActionDescription is Not ControllerActionDescriptor");

            var authenticationService = sp.GetService<IAuthService>();
            if (user.Name == "admin") return;
            if (!await authenticationService.HasAccessAsync(user.Id, policies.ToArray()))
            {
                throw new UnauthorizedAccessException("AccessDenied");
            }
        }

        foreach (var authService in sp.GetServices<IAuthService>())
        {
            if (!await authService.IsTokenValid(context.HttpContext, user.Claims))
            {
                throw new UnauthorizedAccessException("InvalidToken");
            }
        }
    }
}