using System;
using System.Linq;

namespace Devor.Framework.Extensions
{
    public static class RandomExtensions
    {
        public static string NextString(this Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
