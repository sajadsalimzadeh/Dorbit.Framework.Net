using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class MemoryCacheExtensions
{
    private static ConcurrentDictionary<string, object> locks = new();

    public static bool TryGetValueWithLock<T>(this IMemoryCache memoryCache, string key, out T value)
    {
        var lockObj = locks.GetOrAdd(key, new { });
        lock (lockObj)
        {
            return memoryCache.TryGetValue(key, out value);
        }
    }
}