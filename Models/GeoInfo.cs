namespace Dorbit.Framework.Models;

public class GeoInfo
{
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public string Region { get; set; }
    public string RegionName { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lon { get; set; }
    public string Timezone { get; set; }
    public string Isp { get; set; }
    public string Org { get; set; }
    public string As { get; set; }
}