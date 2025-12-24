using System.Linq;
using Dorbit.Framework.Contracts.Identities;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Extensions;

public static class HttpExtensions
{
    public static string GetByNames(this HttpRequest request, string[] names)
    {
        
        var value = string.Empty;
        foreach (var key in names)
        {
            if (request.Query.Keys.Contains(key)) value = request.Query[key];
            else if (request.Query.Keys.Contains(key.ToLower())) value = request.Query[key.ToLower()];
            else if (request.Headers.Keys.Contains(key)) value = request.Headers[key].FirstOrDefault();
            else if (request.Headers.Keys.Contains(key.ToLower())) value = request.Headers[key.ToLower()].FirstOrDefault();
            else if (request.Cookies.Keys.Contains(key)) value = request.Cookies[key];
            else if (request.Cookies.Keys.Contains(key.ToLower())) value = request.Cookies[key.ToLower()];
            if (!string.IsNullOrEmpty(value)) break;
        }

        return value;
    }
    
    public static bool TryGetByNames(this HttpRequest request, string[] names, out string value)
    {
        
        value = request.GetByNames(names);
        return value.IsNotNullOrEmpty();
    }
    
    public static string GetAccessToken(this HttpRequest request)
    {
        var token = request.GetByNames([
            "access_token",
            "ApiKey",
            "Authorization",
        ]);
        if (token is not null) token = token.Replace("Bearer", "").Trim();
        return token;
    }

    public static string GetCsrfToken(this HttpRequest request)
    {
        return request.GetByNames([
            "csrf_token",
            "Csrf-Token",
            "CsrfToken",
        ]);
    }

    public static IdentityRequest GetIdentityRequest(this HttpContext context)
    {
        var request = new IdentityRequest(); 
        request.IpV4 = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        request.IpV6 = context.Connection.RemoteIpAddress?.MapToIPv6().ToString();
        request.UserAgent = context.Request.Headers.FirstValueOrDefault("User-Agent");
        request.AccessToken = context.Request.GetAccessToken();
        request.CsrfToken = context.Request.GetCsrfToken();
        return request;
    }

    public static string GetUserAgent(this HttpRequest request)
    {
        return request.Headers.ContainsKey("User-Agent") ? request.Headers["User-Agent"] : string.Empty;
    }

    public static string GetOrigin(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host.Value}";
    }
}