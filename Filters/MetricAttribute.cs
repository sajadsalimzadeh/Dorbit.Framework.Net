using Dorbit.Framework.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dorbit.Framework.Filters;

public class MetricAttribute<T> : ActionFilterAttribute where T : class
{
    protected readonly T Metric;

    public MetricAttribute(Type type, string memberName)
    {
        Metric = type.GetStaticPropertyValue<T>(memberName);
    }
}