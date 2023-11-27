using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class CacheableExtensions
{
    private static ConcurrentDictionary<string, object> _monitors = new();

    public static Task<TR> WithCacheAsync<T, TR>(this T obj, Func<T, Task<TR>> action, string key, TimeSpan duration)
    {
        key = $"{nameof(WithCacheAsync)}-{typeof(T).Name}-{typeof(TR).Name}-{key}";
        if (App.MemoryCache.TryGetValue(key, out TR result)) return Task.FromResult(result);
        var monitor = _monitors.GetOrAdd(key, (_) => new { });
        lock (monitor)
        {
            if (!App.MemoryCache.TryGetValue(key, out TR _))
            {
                result = action(obj).Result;
                App.MemoryCache.Set(key, result, duration);
            }

            return Task.FromResult(result);
        }
    }
}