using System;
using System.IO;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Loggers;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Hosts;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Dorbit.Framework.Filters;

public class LogRequestAttribute : ActionFilterAttribute
{
    private string DirectoryName { get; }
    private static ConfigLogRequest _config;

    public LogRequestAttribute()
    {
    }

    public LogRequestAttribute(string directoryName)
    {
        DirectoryName = directoryName;
    }

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

        if (DirectoryName is null) return;
        
        _config ??= context.HttpContext.RequestServices.GetService<IOptions<ConfigLogRequest>>().Value;
        var loggerService = context.HttpContext.RequestServices.GetService<LoggerService>();
        if (_config?.BasePath.IsNotNullOrEmpty() != true) return;
        if (Directory.Exists(_config.BasePath)) Directory.CreateDirectory(_config.BasePath);
        var dirPath = Path.Combine(_config.BasePath, DirectoryName);
        if (Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is byte[] bytes)
            {
                loggerService.LogToFile(new LogFileRequest()
                {
                    Path = Path.Combine(dirPath, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-ffff}.txt"),
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