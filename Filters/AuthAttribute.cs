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
            if (controllerAuthAttributes.Any(x => Equals(x, this)))
            {
                if (methodAttributes.Count() > 0) return;
            }

            if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null) return;
        }

        var request = context.HttpContext.Request;
        var sp = context.HttpContext.RequestServices;
        var token = string.Empty;
        var keyNames = new[]
        {
            "Token",
            "ApiKey",
            "Api_Key",
            "Authorization",
        };
        foreach (var key in keyNames)
        {
            if (request.Query.Keys.Contains(key)) token = request.Query[key];
            else if (request.Query.Keys.Contains(key.ToLower())) token = request.Query[key.ToLower()];
            else if (request.Headers.Keys.Contains(key)) token = request.Headers[key].FirstOrDefault();
            else if (request.Headers.Keys.Contains(key.ToLower())) token = request.Headers[key.ToLower()].FirstOrDefault();
            else if (request.Cookies.Keys.Contains(key)) token = request.Cookies[key];
            else if (request.Cookies.Keys.Contains(key.ToLower())) token = request.Cookies[key.ToLower()];
            if (!string.IsNullOrEmpty(token)) break;
        }

        if (string.IsNullOrEmpty(token)) throw new AuthenticationException();

        if (token.Contains("Bearer ")) token = token.Replace("Bearer ", "");
        var userResolver = sp.GetService<IUserResolver>();
        var jwtService = sp.GetService<JwtService>();
        if (await jwtService.TryValidateTokenAsync(token, out _, out var claims))
        {
            var id = claims.FindFirst("UserId")?.Value ?? claims.FindFirst("Id")?.Value;
            userResolver.User = new UserDto()
            {
                Id = Guid.Parse(id ?? ""),
                Name = claims.FindFirst("Name")?.Value
            };
        }

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
            if (!await authService.IsTokenValid(claims))
            {
                throw new UnauthorizedAccessException("InvalidToken");
            }
        }
    }
}