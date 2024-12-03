using Dorbit.Framework.Configs;
using Dorbit.Framework.Utils.Http;
using Serilog;

namespace Dorbit.Framework.Extensions;

public static class ConfigClientExtensions
{
    public static HttpHelper GetHttpHelper(this ConfigClient configClient, string apiKeyHeader = null)
    {
        return configClient.GetHttpHelper(null, apiKeyHeader);
    }
    
    public static HttpHelper GetHttpHelper(this ConfigClient configClient, ILogger logger, string apiKeyHeader = null)
    {
        var http = new HttpHelper(configClient.ApiUrl ?? configClient.BaseUrl);
        if (configClient.ApiKey is not null)
        {
            http.AddHeader(apiKeyHeader ?? "AuthorizationService", configClient.ApiKey.GetDecryptedValue());
        }

        if (logger is not null)
        {
            http.OnException += (ex, req, res) => { logger.Error("Http Client {@Exception} {@Request} {@Response}", ex, req, res); };
        }

        return http;
    }
}