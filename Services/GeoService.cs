using Dorbit.Attributes;
using Dorbit.Models;
using Dorbit.Services.Abstractions;
using Dorbit.Utils.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
internal class GeoService : IGeoService
{
    private HttpHelper GetHttpClient(string ip)
    {
        return new HttpHelper($"http://ip-api.com/json/{ip}");
    }
        
    public Task<HttpModel<GeoInfo>> GetGeoInfoAsync(string ip)
    {
        return GetHttpClient(ip).GetAsync<GeoInfo>();
    }
}