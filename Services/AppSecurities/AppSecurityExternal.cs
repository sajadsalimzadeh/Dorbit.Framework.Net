using System.Reflection;
using Dorbit.Framework.Contracts;

namespace Dorbit.Framework.Services.AppSecurities;

public class AppSecurityExternal : IAppSecurity
{
    private readonly MethodInfo _decryptMethodInfo;
    private readonly MethodInfo _encryptMethodInfo;
    private readonly MethodInfo _getKeyMethodInfo;

    public AppSecurityExternal(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            _decryptMethodInfo = type.GetMethod("Decrypt");
            _encryptMethodInfo = type.GetMethod("Encrypt");
            _getKeyMethodInfo = type.GetMethod("GetKey");
        }
    }

    public byte[] GetKey()
    {
        return _getKeyMethodInfo.Invoke(null, []) as byte[];
    }

    public byte[] Encrypt(string value)
    {
        return _encryptMethodInfo.Invoke(null, [value]) as byte[];
    }

    public string Decrypt(byte[] value)
    {
        return _decryptMethodInfo.Invoke(null, [value]) as string;
    }
}