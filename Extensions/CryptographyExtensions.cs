using Dorbit.Framework.Models.Cryptograpy;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Extensions;

public static class CryptographyExtensions
{
    public static string GetDecryptedValue(this ProtectedProperty property)
    {
        return GetDecryptedValue(property, App.Key);
    }

    public static string GetDecryptedValue(this ProtectedProperty property, byte[] key)
    {
        var algorithmName = property.GetHashAlgorithmName();
        if (algorithmName.Name?.ToLower() == "none")
        {
            return property.Value;
        }

        return new Aes()
        {
            Iterations = property.Iterations,
            HashAlgorithm = algorithmName
        }.Decrypt(property.Value, key).ToStringUtf8();
    }
}