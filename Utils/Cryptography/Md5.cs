using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dorbit.Framework.Utils.Cryptography;

public static class Md5
{
    public static Task<byte[]> HashAsync(Stream stream)
    {
        return MD5.Create().ComputeHashAsync(stream);
    }

    public static Task<byte[]> HashFileAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return HashAsync(stream);
    }
}