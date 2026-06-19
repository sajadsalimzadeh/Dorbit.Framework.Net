using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.OpenWeathers;

public class OpenWeatherDailyResponse
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Timezone { get; set; }
    public int TimezoneOffset { get; set; }
    public List<OpenWeatherDailyResponseData> Data { get; set; }
    
}

public class OpenWeatherDailyResponseData
{
    public long Dt { get; set; }
    public long Sunrise { get; set; }
    public long Sunset { get; set; }
    public long Moonrise { get; set; }
    public long Moonset { get; set; }
    public double MoonPhase { get; set; }
    public double Pressure { get; set; }
    public double Humidity { get; set; }
    public double DewPoint { get; set; }
    public double WindSpeed { get; set; }
    public double WindDeg { get; set; }
    public double WindGust { get; set; }
    public double Clouds { get; set; }
    public double Pop { get; set; }
    public double Uvi { get; set; }
    
    public OpenWeatherDailyResponseDataTemp Temp { get; set; }
    public OpenWeatherDailyResponseDataFeelsLike FeelsLike { get; set; }
    public List<OpenWeatherDailyResponseDataWeather> Weather { get; set; }
    
}

public class OpenWeatherDailyResponseDataTemp
{
    public double Day { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Night { get; set; }
    public double Eve { get; set; }
    public double Morn { get; set; }
}

public class OpenWeatherDailyResponseDataFeelsLike
{
    public double Day { get; set; }
    public double Eve { get; set; }
    public double Morn { get; set; }
    public double Night { get; set; }
}

public class OpenWeatherDailyResponseDataWeather
{
    public int Id { get; set; }
    public string Main { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}