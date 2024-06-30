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

    public string Encrypt(string value)
    {
        return _aes?.Encrypt(value).ToStringUtf8();
    }

    public string Decrypt(string value)
    {
        return _aes?.Decrypt(value).ToStringUtf8();
    }
}