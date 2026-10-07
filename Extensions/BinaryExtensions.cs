using System;
using System.Linq;
using System.Text;

namespace Dorbit.Framework.Extensions;

public static class BinaryExtensions
{
    public static byte[] AddChecksum(this byte[] data, byte iv = 0x5A, byte ev = 0xFF)
    {
        var checksum = iv;
        foreach (var d in data) checksum ^= d;
        checksum ^= ev;
        return data.Append(checksum).ToArray();
    }

    public static byte[] AddChecksumIf(this byte[] data, bool condition, byte iv = 0x5A, byte ev = 0xFF)
    {
        return condition ? data.AddChecksum(iv, ev) : data;
    }

    public static byte[] TrimAes(this byte[] data)
    {
        var i = data.Length;
        for (; i > 0; i--)
        {
            if(data[i - 1] != 0) break;
        }
        return data.Take(i).ToArray();
    }

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

    public static string ToHexString(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
}