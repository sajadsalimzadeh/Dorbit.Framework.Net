using System;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Users;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services;
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
                var userResolver = sp.GetService<IUserResolver>();
                var jwtService = sp.GetService<JwtService>();
                if (await jwtService.TryValidateTokenAsync(token, out _, out var claims))
                {
                    var id = claims.FindFirst("UserId")?.Value ?? claims.FindFirst("Id")?.Value;
                    userResolver.User = new BaseUserDto()
                    {
                        Id = Guid.Parse(id ?? ""),
                        Username = claims.FindFirst("Name")?.Value,
                        Claims = claims,
                    };
                }
            }
        }
        catch
        {
            // ignored
        }

        await next.Invoke(context);
    }
}