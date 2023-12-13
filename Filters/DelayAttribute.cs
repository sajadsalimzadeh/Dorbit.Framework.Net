using Microsoft.AspNetCore.Mvc.Filters;

namespace Dorbit.Framework.Filters;

public class DelayAttribute : ActionFilterAttribute
{
    public int Request { get; set; }
    public int Response { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (Request > 0) Thread.Sleep(Request);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (Response > 0) Thread.Sleep(Response);
    }
}