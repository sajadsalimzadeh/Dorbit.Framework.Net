using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Devor.Framework.Services.Abstractions;

namespace Devor.Framework.Middlewares
{
    public class JsonWebTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public JsonWebTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILoggerService logger, IAuthenticationService authenticationService, IUserResolver userResolver)
        {
            var request = context.Request;
            string token = string.Empty;
            var keyNames = new string[]
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
                try
                {
                    if (token.Contains("Bearer ")) token = token.Replace("Bearer ", "");
                    var claims = authenticationService.GetJwtClaims(token);
                    context.Items["UserId"] = long.Parse(claims.FirstOrDefault(x => x.Type == "id").Value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex);
                }
            }

            await _next(context);
        }
    }
}