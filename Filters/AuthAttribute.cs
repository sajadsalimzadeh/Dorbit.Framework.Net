using System.Reflection;
using System.Security.Authentication;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Models.Users;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using MailKit.Security;
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
        try
        {
<<<<<<< HEAD
            var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
            var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
            if (controllerAuthAttributes.Any(x => Equals(x, this)) && methodAttributes.Any()) return;
            if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null) return;
=======
            if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                var controllerAuthAttributes = actionDescriptor.ControllerTypeInfo.GetCustomAttributes<AuthAttribute>();
                var methodAttributes = actionDescriptor.MethodInfo.GetCustomAttributes<AuthAttribute>();
                if (controllerAuthAttributes.Any(x => Equals(x, this)))
                {
                    if (methodAttributes.Any()) return;
                }

                if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null) return;
            }

            var request = context.HttpContext.Request;
            var services = context.HttpContext.RequestServices;

            //Find Token
            var token = string.Empty;
            var keyNames = new[]
            {
                "Authorization",
                "Token",
                "ApiKey",
                "Api_Key",
            };

            foreach (var key in keyNames)
            {
                if (request.Headers.Keys.Contains(key.ToLower())) token = request.Headers[key.ToLower()].FirstOrDefault();
                else if (request.Headers.Keys.Contains(key)) token = request.Headers[key].FirstOrDefault();
                else if (request.Cookies.Keys.Contains(key)) token = request.Cookies[key];
                else if (request.Cookies.Keys.Contains(key.ToLower())) token = request.Cookies[key.ToLower()];
                else if (request.Query.Keys.Contains(key)) token = request.Query[key];
                else if (request.Query.Keys.Contains(key.ToLower())) token = request.Query[key.ToLower()];

                if (!string.IsNullOrEmpty(token)) break;
            }

            if (string.IsNullOrEmpty(token)) throw new AuthenticationException();

            //Find User
            if (token.Contains("Bearer ")) token = token.Replace("Bearer ", "");
            var jwtService = services.GetService<JwtService>();
            var userResolver = services.GetService<IUserResolver>() ?? throw new Exception($"{nameof(IUserResolver)} not implemented");

            if (!await jwtService.TryValidateTokenAsync(token, out _, out var claims))
            {
                throw new AuthenticationException();
            }

            var user = userResolver.User = new UserDto()
            {
                Id = Guid.Parse(claims.FindFirst("Id")?.Value ?? ""),
                Name = claims.FindFirst("Name")?.Value
            };

            //Add Extra info of client
            var userStateService = services.GetService<IUserStateService>();
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

                var authenticationService = services.GetService<IAuthService>();
                if (user.Name == "admin") return;
                if (!await authenticationService.HasAccessAsync(user.Id, policies.ToArray()))
                {
                    throw new UnauthorizedAccessException("AccessDenied");
                }
            }

            await OnActionExecutionAsync(context, claims);
>>>>>>> f03bf58c1079ab033b0f9a9ad3ec046653d789d6
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

    protected virtual Task OnActionExecutionAsync(ActionExecutingContext context, ClaimsPrincipal claims)
    {
        return Task.CompletedTask;
    }
}