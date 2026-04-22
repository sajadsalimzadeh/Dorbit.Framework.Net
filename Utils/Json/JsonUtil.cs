
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dorbit.Framework.Utils.Json;

public static class JsonUtil
{
    public static string SerializeCamelCase(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    
    public static T DeserializeCamelCase<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    public static T DeserializeFromFile<T>(string path)
    {
        var content = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions.Web);
    }
    
    public static async Task<T> DeserializeFromFileAsync<T>(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions.Web);
    }
}