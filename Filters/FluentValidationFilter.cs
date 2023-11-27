using Microsoft.AspNetCore.Mvc.Filters;

namespace Dorbit.Framework.Filters;

public class FluentValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        base.OnActionExecuted(context);
    }
}