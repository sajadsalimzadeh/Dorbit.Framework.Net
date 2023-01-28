#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable SYSLIB0021 // Type or member is obsolete
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Devor.Framework.Utils.Cryptography
{
    public static class AES
    {
        public static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            byte[] saltBytes = passwordBytes;
            // Example:
            //saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (var aes = new AesManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            // Set your salt here to meet your flavor:
            byte[] saltBytes = passwordBytes;
            // Example:
            //saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (var aes = new AesManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
        public static string Encrypt(string text, string passwordBytes)
        {
            return Encrypt(text, Encoding.ASCII.GetBytes(passwordBytes));
        }
        public static string Encrypt(string text, byte[] passwordBytes)
        {
            var originalBytes = Encoding.UTF8.GetBytes(text);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            var saltSize = GetSaltSize(passwordBytes);

            var saltBytes = GetRandomBytes(saltSize);


            var bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
            for (int i = 0; i < saltBytes.Length; i++)
                bytesToBeEncrypted[i] = saltBytes[i];

            for (int i = 0; i < originalBytes.Length; i++)
                bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];
            byte[] encryptedBytes = Encrypt(bytesToBeEncrypted, passwordBytes);
            return Convert.ToBase64String(encryptedBytes);
        }
        public static string Decrypt(string decryptedText, byte[] passwordBytes)
        {
            var bytesToBeDecrypted = Convert.FromBase64String(decryptedText);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            var decryptedBytes = Decrypt(bytesToBeDecrypted, passwordBytes);
            int saltSize = GetSaltSize(passwordBytes);
            byte[] originalBytes = new byte[decryptedBytes.Length - saltSize];
            for (int i = saltSize; i < decryptedBytes.Length; i++)
                originalBytes[i - saltSize] = decryptedBytes[i];

            return Encoding.UTF8.GetString(originalBytes);
        }
        public static int GetSaltSize(byte[] passwordBytes)
        {

            var key = new Rfc2898DeriveBytes(passwordBytes, passwordBytes, 1000);
            var ba = key.GetBytes(2);
            var sb = new StringBuilder();
            for (int i = 0; i < ba.Length; i++)
            {
                sb.Append(Convert.ToInt32(ba[i]));
            }
            int saltSize = 0;
            string s = sb.ToString();
            foreach (char c in s)
            {
                int intc = Convert.ToInt32(c.ToString());
                saltSize += intc;
            }

            return saltSize;
        }
        public static byte[] GetRandomBytes(int length)
        {
            byte[] ba = new byte[length];
            RandomNumberGenerator.Create().GetBytes(ba);
            return ba;
        }
    }
}
