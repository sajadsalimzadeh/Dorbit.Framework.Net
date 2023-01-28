using Devor.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devor.Framework.Services.Abstractions
{
    public interface IGeoService
    {
        GeoInfo GetGeoInfo(string ip);
        Task<GeoInfo> GetGeoInfoAsync(string ip);
    }
}
