﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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
            if (!App.MemoryCache.TryGetValue(key, out TR cacheResult))
            {
                cacheResult = action(obj).Result;
                App.MemoryCache.Set(key, cacheResult, duration);
            }

            return Task.FromResult(cacheResult);
        }
    }
    
    public static T GetValueOrElse<T>(this IMemoryCache cache, string key, Func<T> elseAction, TimeSpan timeSpan)
    {
        if (!cache.TryGetValue<T>(key, out var result))
        {
            result = elseAction();
            cache.Set(key, result, timeSpan);
        }

        return result;
    }
    
    public static async Task<T> GetValueOrElseAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> elseAction, TimeSpan timeSpan)
    {
        if (!cache.TryGetValue<T>(key, out var result))
        {
            result = await elseAction();
            cache.Set(key, result, timeSpan);
        }

        return result;
    }
}