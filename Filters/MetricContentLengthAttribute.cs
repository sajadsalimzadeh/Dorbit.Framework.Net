﻿using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Prometheus;

namespace Dorbit.Framework.Filters;

public class MetricContentLengthAttribute : MetricAttribute<Summary>
{
    public MetricContentLengthAttribute(Type type, string memberName) : base(type, memberName)
    {
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context?.HttpContext.Request.ContentLength is not null)
        {
            Metric.Observe(context.HttpContext.Request.ContentLength.Value);
        }

        base.OnActionExecuting(context);
    }
}