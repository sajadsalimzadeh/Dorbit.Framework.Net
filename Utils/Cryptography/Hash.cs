using System.Text;

namespace Dorbit.Utils.Cryptography;

public static class Hash
{
    private const string Salt = "yf4m394wpV";
    
    public static string Sha1(string text, string secret)
    {
        var data = Encoding.ASCII.GetBytes(text + secret + Salt);
        var hashData = System.Security.Cryptography.SHA1.Create().ComputeHash(data);
        var hash = string.Empty;
        foreach (var b in hashData)
        {
            hash += b.ToString("X2");
        }
        return hash;
    }
}