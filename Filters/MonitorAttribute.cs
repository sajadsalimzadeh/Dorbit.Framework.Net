using Dorbit.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Filters
{
    public class MonitorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["Monitoring-StartTime"] = DateTime.Now;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var startTime = (DateTime)context.HttpContext.Items["Monitoring-StartTime"];
            var endTime = DateTime.Now;
            var diffTime = endTime - startTime;
            var monitorService = context.HttpContext.RequestServices.GetService<IMonitorService>();
            monitorService.AddResponseDuration(diffTime.Milliseconds);
            monitorService.AddRequest();
        }
    }
}
