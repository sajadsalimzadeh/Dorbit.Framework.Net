using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dorbit.Framework.Utils.Cryptography;

public static class HashUtil
{
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

    public static string PasswordV1(string password, string secretKey)
    {
        return Sha256(password + secretKey + secretKey);
    }

    public static string PasswordV2(string password, string secretKey)
    {
        return Sha256(secretKey + password + secretKey);
    }

    public static string PasswordV3(string password, string secretKey)
    {
        return Sha256(secretKey + secretKey + password);
    }
}