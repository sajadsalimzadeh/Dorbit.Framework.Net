using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Middlewares;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class AuthMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            var sp = context.RequestServices;
            var token = context.Request.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                var userResolver = sp.GetRequiredService<IUserResolver>();
                var authService = sp.GetRequiredService<IIdentityService>();
                userResolver.User = await authService.GetUserByTokenAsync(token);
            }
        }
        catch
        {
            // ignored
        }

        await next.Invoke(context);
    }
}