using System.Security.Cryptography;
using System.Text;

namespace Dorbit.Framework.Utils.Cryptography;

public static class Hash
{
    private const string secret = "yf4m394wpV";

    public static string Sha1(string text)
    {
        var data = Encoding.ASCII.GetBytes(text);
        var hashData = SHA1.Create().ComputeHash(data);
        var hash = string.Empty;
        foreach (var b in hashData)
        {
            hash += b.ToString("X2");
        }

        return hash;
    }

    public static string Sha1(string text, string secretKey)
    {
        return Sha1(text, secretKey + secretKey);
    }

    public static string Sha256(string text)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2")); // x2 formats as hexadecimal
        }

        return builder.ToString();
    }

    public static string Sha256(string text, string secretKey)
    {
        return Sha256(text + secretKey + secretKey);
    }
}