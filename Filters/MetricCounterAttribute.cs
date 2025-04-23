using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Prometheus;

namespace Dorbit.Framework.Filters;

public class MetricCounterAttribute(Type type, string memberName) : MetricAttribute<Summary>(type, memberName)
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items.Add("StartTime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items.TryGetValue("StartTime", out var startTime))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Metric.Observe(now - (long)startTime);
        }

        ;

        base.OnActionExecuted(context);
    }
}