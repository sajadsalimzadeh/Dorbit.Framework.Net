using Dorbit.Framework.Models.Cryptographies;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Extensions;

public static class CryptographyExtensions
{
    public static string GetDecryptedValue(this ProtectedProperty property, byte[] key)
    {
        return Aes.Decrypt(property.Value, key);
    }
}