using System.Reflection;
using Dorbit.Framework.Controllers;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    private IEnumerable<string> _accesses;
    public string Tenant { get; set; }

    public AuthAttribute(params string[] accesses)
    {
        _accesses = accesses;
    }

    public IEnumerable<string> GetAccesses(Type type)
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
            if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null) return;
        }

        var userResolver = context.HttpContext.RequestServices.GetService<IUserResolver>() ??
                           throw new Exception($"{nameof(IUserResolver)} not implemented");
        var user = userResolver.User;
        if (user is null) throw new UnauthorizedAccessException("UnAuthorized");
        else
        {
            var userStateService = context.HttpContext.RequestServices.GetService<IUserStateService>();
            var state = userStateService.GetUserState(user.Id);
            state.Url = context.HttpContext.Request.GetDisplayUrl();
            state.LastRequestTime = DateTime.Now;
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

                var authenticationService = context.HttpContext.RequestServices.GetService<IAuthService>();
                if (user.Name == "admin") return;
                if (!await authenticationService.HasAccessAsync(user.Id, policies.ToArray()))
                {
                    throw new UnauthorizedAccessException("AccessDenied");
                }
            }
        }
    }
}