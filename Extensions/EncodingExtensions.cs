using System;
using System.Linq;
using System.Text;

namespace Dorbit.Framework.Extensions;

public static class EncodingExtensions
{
    public static string ToStringAscii(this byte[] bytes)
    {
        return Encoding.ASCII.GetString(bytes);
    }

    public static string ToStringUtf8(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public static string ToStringUtf32(this byte[] bytes)
    {
        return Encoding.UTF32.GetString(bytes);
    }
    
    public static byte[] ToBytesAscii(this string bytes)
    {
        return Encoding.ASCII.GetBytes(bytes);
    }

    public static byte[] ToBytesUtf8(this string bytes)
    {
        return Encoding.UTF8.GetBytes(bytes);
    }

    public static byte[] ToByteArray(this string bytes)
    {
        return bytes.Select(s => Convert.ToByte(s)).ToArray();
    }

    public static byte[] ToBytesUtf32(this string bytes)
    {
        return Encoding.UTF32.GetBytes(bytes);
    }
}