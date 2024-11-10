using Dorbit.Framework.Contracts;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Services.AppSecurities;

public class AppSecurityInternal : IAppSecurity
{
    private readonly Aes _aes;

    internal AppSecurityInternal(byte[] value)
    {
        _aes = new Aes(value);
    }

    public byte[] GetKey()
    {
        return _aes.Key;
    }

    public byte[] Encrypt(string value)
    {
        return _aes?.Encrypt(value);
    }

    public string Decrypt(byte[] value)
    {
        return _aes?.Decrypt(value).ToStringUtf8().TrimEnd('\0');
    }
}