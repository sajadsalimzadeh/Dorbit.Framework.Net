using System.Text;

namespace Dorbit.Utils.Cryptography
{
    public static class Hash
    {
        const string SALT = "yf4m394wpV";
        public static string SHA1(string text, string secret)
        {
            var data = Encoding.ASCII.GetBytes(text + secret + SALT);
            var hashData = System.Security.Cryptography.SHA1.Create().ComputeHash(data);
            var hash = string.Empty;
            foreach (var b in hashData)
            {
                hash += b.ToString("X2");
            }
            return hash;
        }
        public static string RandomString(int length)
        {
            var random = new System.Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
