using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Filters;

public class CacheAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Cache timeout seconds
    /// </summary>
    public int Timeout { get; set; } = 60;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
        var key = context.HttpContext.Request.GetDisplayUrl();
        if (memoryCache.TryGetValue(key, out var value))
        {
            context.Result = new ObjectResult(value);
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
        var key = context.HttpContext.Request.GetDisplayUrl();
        if (!memoryCache.TryGetValue(key, out var value))
        {
            if (context.Result is ObjectResult obj)
            {
                memoryCache.Set(key, obj.Value, TimeSpan.FromSeconds(Timeout));
            }
        }
    }
}