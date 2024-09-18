using Dorbit.Framework.Configs;
using Dorbit.Framework.Utils.Http;
using Serilog;

namespace Dorbit.Framework.Extensions;

public static class ConfigClientExtensions
{
    public static HttpHelper GetHttpHelper(this ConfigClient configClient)
    {
        return configClient.GetHttpHelper(null);
    }
    
    public static HttpHelper GetHttpHelper(this ConfigClient configClient, ILogger logger)
    {
        var http = new HttpHelper(configClient.ApiUrl ?? configClient.BaseUrl);
        if (configClient.ApiKey is not null)
        {
            http.AddHeader("AuthorizationService", configClient.ApiKey.GetDecryptedValue());
        }

        if (logger is not null)
        {
            http.OnException += (ex, req, res) => { logger.Error("Http Client {@Exception} {@Request} {@Response}", ex, req, res); };
        }

        return http;
    }
}