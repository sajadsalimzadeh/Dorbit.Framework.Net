using System;
using System.Linq;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Dorbit.Framework.Utils.Http;

public abstract class HttpClientApi<T> where T : ConfigClientApi
{
    protected T Config { get; set; }
    protected ILogger Logger { get; }
    protected IHttpContextAccessor HttpContextAccessor { get; }

    public HttpClientApi(IServiceProvider serviceProvider)
    {
        Logger = serviceProvider.GetRequiredService<ILogger>();
        HttpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        Config = serviceProvider.GetRequiredService<IOptions<T>>().Value;
        
        if(Config.ApiUrl.IsNullOrEmpty())
            throw new Exception($"{GetType().Name}: ApiUrl is null or empty");
        
        if(Config.ApiKey is not null && Config.ApiKey.Value.IsNullOrEmpty())
            throw new Exception($"{GetType().Name}: ApiKey.Value is null or empty");
    }

    protected virtual HttpHelper GetHttpHelper()
    {
        var http = Config.GetHttpHelper(Logger);
        http.AuthorizationToken = GetUserToken();
        return http;
    }
    
    
    protected string GetUserToken()
    {
        if (HttpContextAccessor.HttpContext is null || !HttpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var tokenHeader))
        {
            return null;
        }

        return tokenHeader.FirstOrDefault();
    }
}