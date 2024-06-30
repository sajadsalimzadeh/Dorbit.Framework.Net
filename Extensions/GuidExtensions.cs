using System;

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

    public static bool IsNotNullOrEmpty(this Guid? obj)
    {
        return obj.HasValue && obj.Value != Guid.Empty;
    }

    public static bool IsNullOrEmpty(this Guid? obj)
    {
        return !obj.HasValue || obj.Value == Guid.Empty;
    }
}