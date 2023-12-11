using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Filters;

public class MonitorAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items["Monitoring-StartTime"] = DateTime.UtcNow;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var startTime = (DateTime)context.HttpContext.Items["Monitoring-StartTime"];
        var endTime = DateTime.UtcNow;
        var diffTime = endTime - startTime;
        var monitorService = context.HttpContext.RequestServices.GetService<IMonitorService>();
        monitorService.AddResponseDuration(diffTime.Milliseconds);
        monitorService.AddRequest();
    }
}