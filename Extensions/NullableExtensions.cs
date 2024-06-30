namespace Dorbit.Framework.Extensions;

public static class NullableExtensions
{
    public static bool HasNotValue<T>(this T? obj) where T : struct
    {
        return !obj.HasValue;
    }
}