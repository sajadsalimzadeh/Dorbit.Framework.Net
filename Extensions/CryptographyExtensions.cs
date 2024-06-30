using Dorbit.Framework.Contracts.Cryptograpy;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Extensions;

public static class CryptographyExtensions
{
    public static string GetDecryptedValue(this ProtectedProperty property)
    {
        return property.GetDecryptedValue(App.Key);
    }

    public static ProtectedProperty GetEncryptedValue(this string value, Aes aes = null)
    {
        return value.GetEncryptedValue(App.Key, aes);
    }
}