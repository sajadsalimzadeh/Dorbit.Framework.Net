using Devor.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Devor.Framework.Middlewares
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
