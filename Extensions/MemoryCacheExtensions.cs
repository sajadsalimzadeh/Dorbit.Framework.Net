using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class MemoryCacheExtensions
{
    private static readonly ConcurrentDictionary<string, object> Locks = new();

    public static bool TryGetValueWithLock<T>(this IMemoryCache memoryCache, string key, out T value)
    {
        var lockObj = Locks.GetOrAdd(key, new { });
        lock (lockObj)
        {
            return memoryCache.TryGetValue(key, out value);
        }
    }
}