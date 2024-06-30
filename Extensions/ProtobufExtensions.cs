using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Dorbit.Framework.Extensions;

public static class ProtobufExtensions
{
    public static async Task WriteToFileAsync(this IMessage message, string path)
    {
        await using var stream = new FileStream(path, FileMode.OpenOrCreate);
        message.WriteTo(stream);
    }
    

    public static string ToHexString(this ByteString bytes)
    {
        return bytes.ToByteArray().ToHexString();
    }
    
}