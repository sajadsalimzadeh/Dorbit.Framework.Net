using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class MemoryCacheExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public static T GetValueWithLock<T>(this IMemoryCache memoryCache, string key, Func<T> action, TimeSpan timeToLive)
    {
        var semaphoreSlim = Locks.GetOrAdd(key, new SemaphoreSlim(1,1));
        semaphoreSlim.Wait();
        try
        {
            if (!memoryCache.TryGetValue(key, out T value))
            {
                value = action();
                if(value != null) memoryCache.Set(key, value, timeToLive);
            }

            return value;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
    
    public static async Task<T> GetValueWithLockAsync<T>(this IMemoryCache memoryCache, string key, Func<Task<T>> action, TimeSpan timeToLive)
    {
        var semaphoreSlim = Locks.GetOrAdd(key, new SemaphoreSlim(1,1));
        await semaphoreSlim.WaitAsync();
        try
        {
            if (!memoryCache.TryGetValue(key, out T value))
            {
                value = await action();
                memoryCache.Set(key, value, timeToLive);
            }

            return value;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}