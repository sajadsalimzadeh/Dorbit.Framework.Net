using System.Security.Cryptography;
using System.Text;

namespace Dorbit.Utils.Cryptography;

public class Aes
{
    public static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        using var ms = new MemoryStream();
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;

        var key = new Rfc2898DeriveBytes(passwordBytes, passwordBytes, 1000);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        aes.Mode = CipherMode.CBC;

        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            cs.Close();
        }

        var encryptedBytes = ms.ToArray();

        return encryptedBytes;
    }

    public static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        var saltBytes = passwordBytes ?? throw new ArgumentNullException(nameof(passwordBytes));

        using var ms = new MemoryStream();
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;

        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        aes.Mode = CipherMode.CBC;

        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
        cs.Close();

        var decryptedBytes = ms.ToArray();

        return decryptedBytes;
    }

    public static string Encrypt(string text, string passwordBytes)
    {
        return Encrypt(text, Encoding.ASCII.GetBytes(passwordBytes));
    }

    public static string Encrypt(string text, byte[] passwordBytes)
    {
        var originalBytes = Encoding.UTF8.GetBytes(text);
        var sha256 = SHA256.Create();
        passwordBytes = sha256.ComputeHash(passwordBytes);
        var saltSize = GetSaltSize(passwordBytes);

        var saltBytes = GetRandomBytes(saltSize);


        var bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
        for (var i = 0; i < saltBytes.Length; i++)
            bytesToBeEncrypted[i] = saltBytes[i];

        for (var i = 0; i < originalBytes.Length; i++)
            bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];
        var encryptedBytes = Encrypt(bytesToBeEncrypted, passwordBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string decryptedText, byte[] passwordBytes)
    {
        var bytesToBeDecrypted = Convert.FromBase64String(decryptedText);
        var sha256 = SHA256.Create();
        passwordBytes = sha256.ComputeHash(passwordBytes);
        var decryptedBytes = Decrypt(bytesToBeDecrypted, passwordBytes);
        var saltSize = GetSaltSize(passwordBytes);
        var originalBytes = new byte[decryptedBytes.Length - saltSize];
        for (var i = saltSize; i < decryptedBytes.Length; i++)
            originalBytes[i - saltSize] = decryptedBytes[i];

        return Encoding.UTF8.GetString(originalBytes);
    }

    public static int GetSaltSize(byte[] passwordBytes)
    {
        var key = new Rfc2898DeriveBytes(passwordBytes, passwordBytes, 1000);
        var byteArray = key.GetBytes(2);
        var sb = new StringBuilder();
        foreach (var t in byteArray)
        {
            sb.Append(Convert.ToInt32(t));
        }

        var s = sb.ToString();

        return s.Sum(c => Convert.ToInt32(c.ToString()));
    }

    public static byte[] GetRandomBytes(int length)
    {
        var ba = new byte[length];
        RandomNumberGenerator.Create().GetBytes(ba);
        return ba;
    }
}