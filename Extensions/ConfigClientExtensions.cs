﻿using Dorbit.Framework.Configs;
using Dorbit.Framework.Utils.Http;
using Serilog;

namespace Dorbit.Framework.Extensions;

public static class ConfigClientExtensions
{
    public static HttpHelper GetHttpHelper(this ConfigClientApi configClientApi, string apiKeyHeader = null)
    {
        return configClientApi.GetHttpHelper(null, apiKeyHeader);
    }
    
    public static HttpHelper GetHttpHelper(this ConfigClientApi configClientApi, ILogger logger, string apiKeyHeader = null)
    {
        var http = new HttpHelper(configClientApi.ApiUrl ?? configClientApi.BaseUrl);
        if (configClientApi.ApiKey is not null)
        {
            http.AddHeader(apiKeyHeader ?? "AuthorizationService", configClientApi.ApiKey.GetDecryptedValue());
        }

        if (logger is not null)
        {
            http.OnException += (ex, req, res) => { logger.Error("Http Client {@Exception} {@Request} {@Response}", ex, req, res); };
        }

        return http;
    }
}