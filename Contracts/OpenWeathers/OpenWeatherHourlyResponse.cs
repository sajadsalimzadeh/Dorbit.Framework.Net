using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.OpenWeathers;

public class OpenWeatherHourlyResponse
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Timezone { get; set; }

    [JsonPropertyName("timezone_offset")]
    public int TimezoneOffset { get; set; }

    public List<OpenWeatherHourlyResponseData> Data { get; set; }
}

public class OpenWeatherHourlyResponseData
{
    public long Dt { get; set; }

    public double Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    public long Sunrise { get; set; }
    public long Sunset { get; set; }
    public long Moonrise { get; set; }
    public long Moonset { get; set; }
    public double MoonPhase { get; set; }
    public double Pressure { get; set; }
    public double Humidity { get; set; }
    public double DewPoint { get; set; }

    [JsonPropertyName("wind_speed")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("wind_deg")]
    public double WindDegree { get; set; }

    public double WindGust { get; set; }
    public double Clouds { get; set; }
    public double Pop { get; set; }
    public double Uvi { get; set; }
    public List<OpenWeatherHourlyResponseDataWeather> Weather { get; set; }
}

public class OpenWeatherHourlyResponseDataWeather
{
    [JsonPropertyName("id")]
    public OpenWeatherState State { get; set; }

    public string Main { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}