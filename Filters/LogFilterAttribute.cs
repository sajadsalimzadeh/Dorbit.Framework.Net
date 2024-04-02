using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Framework.Filters;

public class LogAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger>();
        if (logger != null)
        {
            var method = context.HttpContext.Request.Method;
            if (method != "GET" && method != "OPTIONS")
            {
                if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                {
                    logger.Information(
                        "Action Executing {@Method} {Controller} {Action} {@Arguments}",
                        method,
                        actionDescriptor.ControllerName,
                        actionDescriptor.ActionName,
                        context.ActionArguments);
                }
            }
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger>();
        if (logger != null)
        {
            var method = context.HttpContext.Request.Method;
            if (method != "GET" && method != "OPTIONS")
            {
                if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                {
                    if (context.Result is ObjectResult objectResult)
                    {
                        logger.Information(
                            "Action Executed {@Method} {Controller} {Action} {@Result}",
                            method,
                            actionDescriptor.ControllerName,
                            actionDescriptor.ActionName,
                            objectResult.Value);
                    }
                }
            }
        }

        base.OnActionExecuted(context);
    }
}