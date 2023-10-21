using Dorbit.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Middlewares
{
    public class CancellationTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public CancellationTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var cancellationTokenService = httpContext.RequestServices.GetService<ICancellationTokenService>();
            if (cancellationTokenService is not null)
            {
                cancellationTokenService.CancellationToken = httpContext.RequestAborted;
            }
            await _next(httpContext);
        }
    }
}
