﻿using System;
using System.Text;
using Google.Protobuf;

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

    public static byte[] ToBytesAscii(this string input)
    {
        return Encoding.ASCII.GetBytes(input);
    }

    public static byte[] ToBytesUtf8(this string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }

    public static byte[] ToByteArray(this string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }

    public static ByteString ToByteString(this string input)
    {
        return ByteString.CopyFrom(input.ToByteArray());
    }

    public static byte[] ToBytesUtf32(this string input)
    {
        return Encoding.UTF32.GetBytes(input);
    }

    public static string ToHexString(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static byte[] FromBase64String(this string str)
    {
        return Convert.FromBase64String(str);
    }
}