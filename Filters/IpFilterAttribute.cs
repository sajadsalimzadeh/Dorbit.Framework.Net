using System;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Filters;

public class IpFilterAttribute(params string[] groupNames) : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipV4 = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? throw new Exception("Ip not recognized");
        var ipFilterConfig = context.HttpContext.RequestServices.GetService<IOptions<ConfigIpFilter>>()?.Value ?? throw new Exception("No Configuration for ip filter");

        if (!ipFilterConfig.Groups.Any(x => groupNames.Contains(x.Key) && x.Value.Contains(ipV4)))
            throw new UnauthorizedAccessException("forbidden ip");
        
        return base.OnActionExecutionAsync(context, next);
    }
}