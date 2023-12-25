using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Framework.Middlewares;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class AuthMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var request = context.Request;
        var token = string.Empty;
        var keyNames = new[]
        {
            "Token",
            "ApiKey",
            "Api_Key",
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
            var logger = context.RequestServices.GetService<ILogger>();
            try
            {
                if (token.Contains("Bearer ")) token = token.Replace("Bearer ", "");
                var userResolver = context.RequestServices.GetService<IUserResolver>();
                userResolver.User = await userResolver.GetUserByTokenAsync(token);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        await next(context);
    }
}