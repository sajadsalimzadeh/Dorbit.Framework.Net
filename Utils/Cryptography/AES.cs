using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dorbit.Framework.Extensions;

namespace Dorbit.Framework.Utils.Cryptography;

public class Aes
{
    public int Iterations { get; set; } = 1000;
    public HashAlgorithmName HashAlgorithm { get; set; } = HashAlgorithmName.SHA1;

    
    public byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] password)
    {
        using var ms = new MemoryStream();
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;

        var key = new Rfc2898DeriveBytes(password, password, Iterations, HashAlgorithm);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        aes.Mode = CipherMode.CBC;

        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
            cs.Close();
        }

        var encryptedBytes = ms.ToArray();

        return encryptedBytes;
    }
    
    public byte[] Encrypt(string text, string password)
    {
        return Encrypt(text, Encoding.ASCII.GetBytes(password));
    }

    public byte[] Encrypt(string text, byte[] password)
    {
        return Encrypt(Encoding.UTF8.GetBytes(text), password);
    }

    public byte[] Decrypt(byte[] text, byte[] password)
    {
        var saltBytes = password ?? throw new ArgumentNullException(nameof(password));

        using var ms = new MemoryStream();
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;

        var key = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithm);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        aes.Mode = CipherMode.CBC;

        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(text, 0, text.Length);
        cs.Close();

        var decryptedBytes = ms.ToArray();

        return decryptedBytes;
    }

    public byte[] Decrypt(string text, byte[] password)
    {
        return Decrypt(text.ToBytesUtf8(), password);
    }

    public byte[] Decrypt(string text, string password)
    {
        return Decrypt(text.ToBytesUtf8(), password.ToBytesUtf8());
    }
}