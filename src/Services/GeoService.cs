using Devor.Framework.Attributes;
using Devor.Framework.Models;
using Devor.Framework.Services.Abstractions;
using Devor.Framework.Utils.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Devor.Framework.Services
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class GeoService : IGeoService
    {
        private HttpClient GetHttpClient(string ip)
        {
            return new HttpClient($"http://ip-api.com/json/{ip}");
        }
        public GeoInfo GetGeoInfo(string ip)
        {
            return GetHttpClient(ip).Get<GeoInfo>();
        }
        public Task<GeoInfo> GetGeoInfoAsync(string ip)
        {
            return GetHttpClient(ip).GetAsync<GeoInfo>();
        }
    }
}
