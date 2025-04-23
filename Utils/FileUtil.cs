using System.IO;
using System.Text.Json;

namespace Dorbit.Framework.Utils;

public static class FileUtil
{
    public static T ReadJson<T>(string filename)
    {
        return JsonSerializer.Deserialize<T>(File.ReadAllText(filename));
    }
}