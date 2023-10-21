using Dorbit.Models.Cryptographies;
using Dorbit.Utils.Cryptography;

namespace Dorbit.Extensions;

public static class CryptographyExtensions
{
    public static string GetDecryptedValue(this ProtectedProperty property, byte[] key)
    {
        return Aes.Decrypt(property.Value, key);
    }
}