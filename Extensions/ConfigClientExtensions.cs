using Dorbit.Framework.Configs;
using Dorbit.Framework.Utils.Http;

namespace Dorbit.Framework.Extensions;

public static class ConfigClientExtensions
{
    public static HttpHelper GetHttpHelper(this ConfigClient configClient)
    {
        var http = new HttpHelper(configClient.ApiUrl ?? configClient.BaseUrl);
        if (configClient.ApiKey is not null)
        {
            http.AddHeader("AuthorizationService", configClient.ApiKey.GetDecryptedValue());
        }

        return http;
    }
}