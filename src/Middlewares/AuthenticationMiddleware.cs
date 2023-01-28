using Devor.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Devor.Framework.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserResolver userResolver)
        {
            var userId = context.Items["UserId"];
            if(userId is not null) userResolver.SetUserId(Convert.ToInt64(userId));
            await _next(context);
        }
    }
}
