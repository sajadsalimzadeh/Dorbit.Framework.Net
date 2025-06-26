using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Middlewares;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class CancellationTokenMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cancellationTokenService = context.RequestServices.GetService<ICancellationTokenService>();
        if (cancellationTokenService is not null)
        {
            cancellationTokenService.RequestCancellationToken = context.RequestAborted;
        }

        return next(context);
    }
}