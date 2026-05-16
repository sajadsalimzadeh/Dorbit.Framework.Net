using Dorbit.Framework.Mappers;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

public class RequestContextFilter: IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is IRequestUserContext userContext)
            {
                var httpContext = context.HttpContext;
                var identityService = httpContext.RequestServices.GetRequiredService<IIdentityService>();

                userContext.RequestUserId = identityService.Identity.User.GetId();
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}