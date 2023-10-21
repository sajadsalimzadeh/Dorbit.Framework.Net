using Dorbit.Models;
using Dorbit.Utils.Http;

namespace Dorbit.Services.Abstractions
{
    public interface IGeoService
    {
        Task<HttpModel<GeoInfo>> GetGeoInfoAsync(string ip);
    }
}
