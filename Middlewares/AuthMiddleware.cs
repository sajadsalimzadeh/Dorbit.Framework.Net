using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Models.Users;
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
        var request = context.Request;
        var sp = context.RequestServices;
        var token = string.Empty;
        var keyNames = new[]
        {
            "access_token",
            "Authorization",
        };
        foreach (var key in keyNames)
        {
            if (request.Query.Keys.Contains(key)) token = request.Query[key];
            else if (request.Query.Keys.Contains(key.ToLower())) token = request.Query[key.ToLower()];
            else if (request.Headers.Keys.Contains(key)) token = request.Headers[key].FirstOrDefault();
            else if (request.Headers.Keys.Contains(key.ToLower())) token = request.Headers[key.ToLower()].FirstOrDefault();
            else if (request.Cookies.Keys.Contains(key)) token = request.Cookies[key];
            else if (request.Cookies.Keys.Contains(key.ToLower())) token = request.Cookies[key.ToLower()];
            if (!string.IsNullOrEmpty(token)) break;
        }

        if (!string.IsNullOrEmpty(token))
        {
            if (token.Contains("Bearer ")) token = token.Replace("Bearer ", "");
            var userResolver = sp.GetService<IUserResolver>();
            var jwtService = sp.GetService<JwtService>();
            if (await jwtService.TryValidateTokenAsync(token, out _, out var claims))
            {
                var id = claims.FindFirst("UserId")?.Value ?? claims.FindFirst("Id")?.Value;
                userResolver.User = new UserDto()
                {
                    Id = Guid.Parse(id ?? ""),
                    Name = claims.FindFirst("Name")?.Value,
                    Claims = claims,
                };
            }
        }

        await next.Invoke(context);
    }
}