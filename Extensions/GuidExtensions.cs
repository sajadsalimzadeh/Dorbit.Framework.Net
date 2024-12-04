using System;
using System.Security.Cryptography;

namespace Dorbit.Framework.Extensions;

public static class GuidExtensions
{
    public static bool IsEmpty(this Guid guid)
    {
        return guid == Guid.Empty;
    }

    public static bool IsNotEmpty(this Guid guid)
    {
        return guid != Guid.Empty;
    }

    public static bool IsNotNullOrEmpty(this Guid? guid)
    {
        return guid.HasValue && guid.Value != Guid.Empty;
    }

    public static bool IsNullOrEmpty(this Guid? guid)
    {
        return !guid.HasValue || guid.Value == Guid.Empty;
    }

    public static long ToLong(this Guid guid)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(guid.ToByteArray());
        return BitConverter.ToInt64(hash, 0);
    }
}