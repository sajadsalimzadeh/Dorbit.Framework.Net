using System.Net;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class GeoService : IGeoService
{
    private HttpHelper GetHttpClient(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip, out var address))
            throw new OperationException(FrameworkErrors.InvalidIpAddress);

        return new HttpHelper($"http://ip-api.com/json/{address}");
    }

    public Task<HttpModel<GeoInfo>> GetGeoInfoAsync(string ip)
    {
        return GetHttpClient(ip).GetAsync<GeoInfo>("");
    }
}