using Dorbit.Framework.Configs;
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
        var http = new HttpHelper(configClientApi.ApiUrl);
        if (configClientApi.ApiKey is not null)
        {
            http.AuthorizationToken = configClientApi.ApiKey.GetDecryptedValue();
        }

        if (logger is not null)
        {
            http.OnException += (ex, args) => { logger.Error("Http Client {@Exception} {@Request} {@Response} {@Content}", ex, args.Request, args.Response, args.Content); };
        }

        return http;
    }
}