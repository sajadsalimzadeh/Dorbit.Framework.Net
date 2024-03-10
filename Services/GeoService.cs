using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

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