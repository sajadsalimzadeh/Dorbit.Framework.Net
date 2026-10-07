using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Dorbit.Framework.Utils.Cryptography;

public static class Md5Util
{
    public static Task<byte[]> HashAsync(Stream stream)
    {
        using var md5 = MD5.Create();
        return Task.FromResult(md5.ComputeHash(stream));
    }

    public static Task<byte[]> HashFileAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return HashAsync(stream);
    }
}
