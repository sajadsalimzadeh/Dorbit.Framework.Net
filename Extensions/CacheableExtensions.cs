using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class CacheableExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Monitors = new();

    public static async Task<Tr> WithCacheAsync<T, Tr>(this T obj, Func<T, Task<Tr>> action, string key, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        key = $"{nameof(WithCacheAsync)}-{typeof(T).Name}-{typeof(Tr).Name}-{key}";

        if (App.MemoryCache.TryGetValue(key, out Tr result))
            return result;

        var semaphore = Monitors.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken.WithAppStoppingToken());

        try
        {
            if (App.MemoryCache.TryGetValue(key, out result))
                return result;

            result = await action(obj);

            App.MemoryCache.Set(key, result, duration);

            return result;
        }
        finally
        {
            semaphore.Release();
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

    public static bool IsTooMuchRequest(this IMemoryCache cache, string key, TimeSpan timeSpan, int limitCount)
    {
        var items = cache.GetOrCreate(key, _ => new List<DateTime>());
        var diffTime = DateTime.UtcNow.Add(-timeSpan);
        items.RemoveAll(x => x < diffTime);
        if (items.Count >= limitCount) return true;
        items.Add(DateTime.UtcNow);
        return false;
    }
}