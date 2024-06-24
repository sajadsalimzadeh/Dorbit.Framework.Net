using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dorbit.Framework.Utils.Json;

public static class JsonUtil
{
    public static string SerializeCamelCase(object obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }
}