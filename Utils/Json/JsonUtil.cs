
using System.Text.Json;
using System.Text.Json.Serialization;

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
}