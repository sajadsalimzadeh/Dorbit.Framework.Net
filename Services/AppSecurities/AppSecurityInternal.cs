using Dorbit.Framework.Contracts;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Services.AppSecurities;

public class AppSecurityInternal : IAppSecurity
{
    private readonly AesHelper _aesHelper;

    internal AppSecurityInternal(byte[] value)
    {
        _aesHelper = new AesHelper(value);
    }

    public byte[] GetKey()
    {
        return _aesHelper.Key;
    }

    public byte[] Encrypt(string value)
    {
        return _aesHelper?.Encrypt(value);
    }

    public string Decrypt(byte[] value)
    {
        return _aesHelper?.Decrypt(value).ToStringUtf8().TrimEnd('\0');
    }
}