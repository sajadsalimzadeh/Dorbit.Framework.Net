using System;
using Dorbit.Framework.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dorbit.Framework.Filters;

public class MetricAttribute<T>(Type type, string memberName) : ActionFilterAttribute
    where T : class
{
    protected readonly T Metric = type.GetStaticPropertyValue<T>(memberName);
}