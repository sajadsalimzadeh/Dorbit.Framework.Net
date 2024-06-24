using System;
using Dorbit.Framework.Contracts.Cryptograpy;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Extensions;

public static class CryptographyExtensions
{
    public static string GetDecryptedValue(this ProtectedProperty property)
    {
        if (App.Security.Decrypt is not null)
        {
            return App.Security.Decrypt(property.Value);
        }
        
        return GetDecryptedValue(property, App.Security.Key);
    }

    public static string GetDecryptedValue(this ProtectedProperty property, byte[] key)
    {
        var algorithmName = property.GetHashAlgorithmName();
        if (algorithmName.Name?.ToLower() == "none")
        {
            return property.Value;
        }

        var encryptedText = Convert.FromBase64String(property.Value);
        var aes = new Aes(key)
        {
            Iterations = property.Iterations,
            HashAlgorithm = algorithmName
        };
        var decryptedText = aes.Decrypt(encryptedText);
        return decryptedText.Trim().ToStringUtf8();
    }

    public static ProtectedProperty GetEncryptedValue(this string value, Aes aes = null)
    {
        if (App.Security.Encrypt is not null)
        {
            var cipherText = App.Security.Encrypt(value);
            return new ProtectedProperty(cipherText)
            {
                Algorithm = "Custom"
            };
        }
        
        return GetEncryptedValue(value, App.Security.Key, aes);
    }

    public static ProtectedProperty GetEncryptedValue(this string value, byte[] key, Aes aes = null)
    {
        aes ??= new Aes(key);
        return new ProtectedProperty()
        {
            Iterations = aes.Iterations,
            Algorithm = aes.HashAlgorithm.ToString(),
            Value = Convert.ToBase64String(aes.Encrypt(value))
        };
    }
}