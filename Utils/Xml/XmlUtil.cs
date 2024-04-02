using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Dorbit.Framework.Utils.Xml;

public static class XmlUtil
{
    public static async Task<T> ParseFileAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        var serializer = new XmlSerializer(typeof(T));
        var content = await File.ReadAllTextAsync(path, cancellationToken);
        using var reader = new StringReader(content);
        return (T)serializer.Deserialize(reader);
    }
}