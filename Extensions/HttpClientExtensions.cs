using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dorbit.Framework.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T> PostAsJsonAsync<T>(this HttpClient httpClient, string url, object value)
    {
        var responseMessage = await httpClient.PostAsJsonAsync(url, value);
        if (responseMessage.IsSuccessStatusCode)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions.Web);
        }

        return default;
    }
}