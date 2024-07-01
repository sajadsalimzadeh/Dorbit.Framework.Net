using System.IO;
using Newtonsoft.Json;

namespace Dorbit.Framework.Utils;

public static class FileUtil
{
    public static T ReadJson<T>(string filename)
    {
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
    }
}