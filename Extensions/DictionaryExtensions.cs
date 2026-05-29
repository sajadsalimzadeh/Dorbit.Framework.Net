using System.Collections.Generic;

namespace Dorbit.Framework.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
    {
        if (dict != null && dict.TryGetValue(key, out var value)) return value;
        return default;
    }
}