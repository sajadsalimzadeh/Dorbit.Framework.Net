using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;

namespace Dorbit.Framework.Utils.Cryptography;

public class Aes
{
    public enum Size : short
    {
        Aes128 = 128
    }

    private readonly short _size;
    private readonly ICryptoTransform _encryptor;
    private readonly ICryptoTransform _decryptor;
    public int Iterations { get; set; } = 1000;
    public HashAlgorithmName HashAlgorithm { get; set; } = HashAlgorithmName.SHA1;
    public PaddingMode PaddingMode { get; init; } = PaddingMode.Zeros;
    public CipherMode CipherMode { get; init; } = CipherMode.CBC;
    public byte[] Key { get; }
    public byte[] Iv { get; }

    private Aes(Size size = Size.Aes128)
    {
        _size = (short)size;
    }

    public Aes(byte[] password, Size size = Size.Aes128) : this(size)
    {
        var key = new Rfc2898DeriveBytes(password, password, Iterations, HashAlgorithm);
        Key = key.GetBytes(_size / 8);
        Iv = key.GetBytes(_size / 8);

        var aes = Create();
        _encryptor = aes.CreateEncryptor();
        _decryptor = aes.CreateDecryptor();
    }

    public Aes(string password, Size size = Size.Aes128) : this(password.ToByteArray(), size)
    {
    }

    public Aes(byte[] key, byte[] iv, Size size = Size.Aes128) : this(size)
    {
        Key = key;
        Iv = iv;

        if (key.Length != _size / 8) throw new OperationException(Errors.AesKeySizeIsInvalid);
        if (key.Length != Iv.Length) throw new OperationException(Errors.AesKeySizeMostEqualIvSize);

        var aes = Create();
        _encryptor = aes.CreateEncryptor();
        _decryptor = aes.CreateDecryptor();
    }

    public Aes(string key, string iv, Size size = Size.Aes128) : this(key.ToByteArray(), iv.ToByteArray(), size)
    {
    }

    private System.Security.Cryptography.Aes Create()
    {
        var aes = System.Security.Cryptography.Aes.Create();
        aes.KeySize = _size;
        aes.BlockSize = _size;
        aes.Padding = PaddingMode.Zeros;
        aes.Key = Key;
        aes.IV = Iv;
        return aes;
    }


    public byte[] Encrypt(byte[] data)
    {
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, _encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return ms.ToArray();
    }

    public byte[] Encrypt(string text)
    {
        return Encrypt(Encoding.UTF8.GetBytes(text));
    }

    public byte[] Decrypt(byte[] text)
    {
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, _decryptor, CryptoStreamMode.Write);
        cryptoStream.Write(text, 0, text.Length);
        cryptoStream.Close();
        return ms.ToArray();
    }

    public byte[] Decrypt(string text)
    {
        return Decrypt(text.ToBytesUtf8());
    }
}