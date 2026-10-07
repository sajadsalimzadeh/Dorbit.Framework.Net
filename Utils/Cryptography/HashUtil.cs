using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Dorbit.Framework.Utils.Cryptography;

public static class HashUtil
{
    public static string Sha1(string text)
    {
        return Sha1(Encoding.ASCII.GetBytes(text));
    }
    
    public static string Sha1(byte[] bytes)
    {
        using var sha1 = SHA1.Create();
        var hashData = sha1.ComputeHash(bytes);
        var hash = string.Empty;
        foreach (var b in hashData)
        {
            hash += b.ToString("X2");
        }

        return hash;
    }

    public static string Sha256(string data)
    {
        return Sha256(Encoding.UTF8.GetBytes(data));
    }

    public static string Sha256(byte[] data)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(data);
        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2")); // x2 formats as hexadecimal
        }

        return builder.ToString();
    }

    public static string PasswordV1(string password, string secretKey)
    {
        return Sha1(password + secretKey + secretKey);
    }

    public static string PasswordV2(string password, string secretKey)
    {
        return Sha1(secretKey + password + secretKey);
    }

    public static string PasswordV3(string password, string secretKey)
    {
        return Sha1(secretKey + secretKey + password);
    }

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256$100000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string stored)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored)) return false;

        var parts = stored.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256") return false;
        if (!int.TryParse(parts[1], out var iterations) || iterations < 100_000) return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
    
    public static string Md5(string input, string salt = "")
    {
        byte[] data;
        using (var md5Hash = MD5.Create())
        {
            data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input + salt));
        }
        var sBuilder = new StringBuilder();

        foreach (var t in data)
        {
            sBuilder.Append(t.ToString("x2"));
        }
        return sBuilder.ToString();
    }
    
    public static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        var output = new List<byte>();
        int bitBuffer = 0, bitsLeft = 0;

        foreach (var c in base32.Replace("=", ""))
        {
            var val = alphabet.IndexOf(char.ToUpperInvariant(c));
            if (val < 0) continue;

            bitBuffer = (bitBuffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                output.Add((byte)((bitBuffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }

        return output.ToArray();
    }
}
