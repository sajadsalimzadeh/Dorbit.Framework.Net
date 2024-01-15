using System;
using System.Linq;

namespace Dorbit.Framework.Extensions;

public static class RandomExtensions
{
    private const string Numbers = "0123456789";
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
    public static string NextNumber(this Random random, int length)
    {
        return new string(Enumerable.Repeat(Numbers, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
        
    public static string NextString(this Random random, int length)
    {
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}