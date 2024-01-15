using System.Threading.Tasks;
using Dorbit.Framework.Models;
using Dorbit.Framework.Utils.Http;

namespace Dorbit.Framework.Services.Abstractions;

public interface IGeoService
{
    Task<HttpModel<GeoInfo>> GetGeoInfoAsync(string ip);
}