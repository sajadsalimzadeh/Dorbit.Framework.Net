using Dorbit.Framework.Contracts.Cryptograpy;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;

namespace Dorbit.Framework.Configs;

public abstract class ConfigClient
{
    public string BaseUrl { get; set; }
    public string ApiUrl { get; set; }
    public ProtectedProperty ApiKey { get; set; }

    public HttpHelper GetHttpHelper()
    {
        var http = new HttpHelper((ApiUrl ?? BaseUrl).Replace("Authorize", ""));
        if (ApiKey is not null)
        {
            http.AddHeader("Authorization", ApiKey.GetDecryptedValue());
        }

        return http;
    }
}