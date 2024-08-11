using System;
using System.IO;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Hosts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Dorbit.Framework.Filters;

public class LogRequestAttribute : ActionFilterAttribute
{
    private static ConfigLogRequest _config;

    public bool LogBody { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger>();
        if (logger == null) return;
        var method = context.HttpContext.Request.Method;
        if (method is "GET" or "OPTIONS") return;
        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            logger.Information(
                "Action Executing {@Method} {Controller} {Action} {@Arguments}",
                method, actionDescriptor.ControllerName, actionDescriptor.ActionName, context.ActionArguments
            );
        }

        if (!LogBody) return;
        _config ??= context.HttpContext.RequestServices.GetService<IOptions<ConfigLogRequest>>().Value;
        if (_config?.BasePath.IsNotNullOrEmpty() != true) return;
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is byte[] bytes)
            {
                FileLoggerHost.Add(new FileLoggerHost.LogRequest()
                {
                    Path = Path.Combine(_config.BasePath, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-ffff}.txt"),
                    Content = bytes
                });
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
                if (context is { ActionDescriptor: ControllerActionDescriptor actionDescriptor, Result: ObjectResult objectResult })
                {
                    logger.Information(
                        "Action Executed {@Method} {Controller} {Action} {@Result}",
                        method, actionDescriptor.ControllerName, actionDescriptor.ActionName, objectResult.Value
                    );
                }
            }
        }

        base.OnActionExecuted(context);
    }
}