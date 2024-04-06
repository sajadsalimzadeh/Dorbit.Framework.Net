using System;
using System.Linq;

namespace Dorbit.Framework.Extensions;

public static class StringExtensions
{
    private static readonly char[] HexChars = ['A', 'B', 'C', 'D', 'E', 'F'];

    public static string ToCamelCase(this string str)
    {
        return string.Concat(str[..1].ToLower(), str.AsSpan(1));
    }

    public static int ToInt32(this string input, bool hex = false)
    {
        input = input?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return 0;
        }

        var spaceIndex = input.IndexOf(' ');
        if (spaceIndex > -1) input = input.Substring(0, spaceIndex);

        if (input.StartsWith("0x"))
        {
            return (int)new System.ComponentModel.Int32Converter().ConvertFromString(input)!;
        }

        if (hex || HexChars.Any(input.Contains))
        {
            return Convert.ToInt32(input, 16);
        }

        return Convert.ToInt32(input);
    }

    public static string ToStringBy(this string format, params object[] args)
    {
        return string.Format(format, args);
    }
}