using System.Reflection;
using Dorbit.Controllers;
using Dorbit.Services.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthAttribute : Attribute, IAuthorizationFilter
    {
        private IEnumerable<string> Policies;
        public string Tenant { get; set; }

        public AuthAttribute(params string[] policies)
        {
            Policies = policies;
        }

        public IEnumerable<string> GetPolicies(Type type)
        {
            var policies = Policies.ToList();
            Type preType = type;
            while (type is not null)
            {
                if (type.IsAssignableFrom(typeof(CrudController)))
                {
                    var entityType = preType.GenericTypeArguments.FirstOrDefault();
                    if (entityType is not null)
                    {
                        policies = policies.ConvertAll(x => x.Replace("[entity]", entityType.Name));
                    }
                    break;
                }
                preType = type;
                type = type.BaseType;
            }
            return policies.Select(x => x.ToLower());
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthIgnoreAttribute>() != null) return;
            }
            var userResolver = context.HttpContext.RequestServices.GetService<IUserResolver>();
            var user = userResolver.GetUser();
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
                userStateService.LoadGeoInfo(state, context.HttpContext.Connection.RemoteIpAddress.ToString());
                if (Policies?.Count() > 0)
                {
                    IEnumerable<string> policies;
                    if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        policies = GetPolicies(controllerActionDescriptor.ControllerTypeInfo);
                    }
                    else throw new Exception("ActionDescription is Not ControllerActionDescriptor");
                    var authenticationService = context.HttpContext.RequestServices.GetService<IAuthenticationService>();
                    if (!authenticationService.HasPolicy(policies.ToArray()))
                    {
                        throw new UnauthorizedAccessException("AccessDenied");
                    }
                }
            }
        }
    }
}
