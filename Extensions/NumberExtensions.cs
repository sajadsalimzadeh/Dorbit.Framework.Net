using System;

namespace Dorbit.Framework.Extensions;

public static class NumberExtensions
{
    public static string ToHex(this int number)
    {
        var hex = number.ToString("X");
        return hex.Length % 2 == 0 ? hex : "0" + hex;
    }
    
    public static string ToHex(this byte number)
    {
        var hex = number.ToString("X");
        return hex.Length % 2 == 0 ? hex : "0" + hex;
    }
    
    public static int Round(this int number, double coefficient)
    {
        return (int)(Math.Round(number / coefficient) * coefficient);
    }
    
    public static long Round(this long number, double coefficient)
    {
        return (long)(Math.Round(number / coefficient) * coefficient);
    }
}