using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Extensions;

public static class HttpExtensions
{
    public static string GetToken(this HttpRequest request)
    {
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

        if (token is not null) token = token.Replace("Bearer ", "");

        return token;
    }
}