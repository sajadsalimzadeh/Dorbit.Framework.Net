using System;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.OpenWeathers;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class OpenWeatherService(IOptions<ConfigOpenWeather> configOpenWeatherOptions)
{
    private string _apiKey = configOpenWeatherOptions.Value.ApiKey.GetDecryptedValue();

    private HttpHelper GetHttpHelper()
    {
        return configOpenWeatherOptions.Value.GetHttpHelper();
    }

    public Task<HttpModel<OpenWeatherDailyResponse>> GetHourlyTimelineAsync(double lat, double lon, DateTime dateTime, int count = 24)
    {
        return GetHttpHelper().GetAsync<OpenWeatherDailyResponse>($"data/4.0/onecall/timeline/1h", new
        {
            cnt = count,
            lat = lat,
            lon = lon,
            start = dateTime.Date.GetUnixTimeSeconds(),
            appid = _apiKey
        });
    }
}