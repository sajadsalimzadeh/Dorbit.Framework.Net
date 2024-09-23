using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dorbit.Framework.Extensions;

public static class StringExtensions
{
    private static readonly char[] HexChars = ['A', 'B', 'C', 'D', 'E', 'F'];

    public static string ToCamelCase(this string str)
    {
        return string.Concat(str[..1].ToLower(), str[1..]);
    }

    public static byte[] HexToByteArray(this string str)
    {
        var arr = str.Replace("-", "");
        var array = new byte[arr.Length / 2];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = Convert.ToByte(arr.Substring(i * 2, 2), 16);
        }

        return array;
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

        try
        {
            if (hex || HexChars.Any(input.Contains))
            {
                return Convert.ToInt32(input, 16);
            }

            return Convert.ToInt32(input);
        }
        catch
        {
            return 0;
        }
    }

    public static string ToStringBy(this string format, params object[] args)
    {
        return string.Format(format, args);
    }

    public static string ReplaceToEmpty(this string value, params string[] toReplace)
    {
        foreach (var t in toReplace)
        {
            value = value.Replace(t, string.Empty);
        }

        return value;
    }

    public static List<string> ReplaceToEmpty(this List<string> array, params string[] toReplace)
    {
        for (var i = 0; i < array.Count; i++)
        {
            array[i] = array[i].ReplaceToEmpty(toReplace);
        }

        return array;
    }

    public static string TrimEnd(this string value, string needle)
    {
        var toTrimLength = needle.Length;
        if (value.Substring((value.Length - toTrimLength), toTrimLength) == needle)
        {
            value = value.Remove((value.Length - toTrimLength));
        }

        return value;
    }

    public static List<string> TrimEnd(this List<string> array, string needle)
    {
        var trimArray = new List<string>();
        for (var i = 0; i < array.Count; i++)
        {
            array[i] = array[i].TrimEnd(needle);
            trimArray.Add(array[i]);
        }

        return trimArray;
    }

    public static string Repeat(this string template, int count)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            sb.Append(template);
        }

        return sb.ToString();
    }

    public static string Reverse(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var array = new char[value.Length];
        var forward = 0;
        for (var i = value.Length - 1; i >= 0; i--)
        {
            array[forward++] = value[i];
        }

        return new string(array);
    }

    public static string ReplacePersianNumbers(this string str)
    {
        return str.Replace("۰", "0")
            .Replace("۱", "1")
            .Replace("۲", "2")
            .Replace("۳", "3")
            .Replace("۴", "4")
            .Replace("۵", "5")
            .Replace("۶", "6")
            .Replace("۷", "7")
            .Replace("۸", "8")
            .Replace("۹", "9")
            .Replace("٠", "0")
            .Replace("١", "1")
            .Replace("٢", "2")
            .Replace("٣", "3")
            .Replace("٤", "4")
            .Replace("٥", "5")
            .Replace("٦", "6")
            .Replace("٧", "7")
            .Replace("٨", "8")
            .Replace("٩", "9");
    }


    public static string ConvertFileContentToBase64(this string imagePath)
    {
        string imageAsBase64 = null;

        if (!string.IsNullOrEmpty(imagePath))
            imageAsBase64 = Convert.ToBase64String(File.ReadAllBytes(imagePath));

        return imageAsBase64;
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNotNullOrEmpty(this string value)
    {
        return !string.IsNullOrEmpty(value?.Trim());
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}