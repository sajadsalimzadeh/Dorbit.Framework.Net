using System.Linq.Expressions;
using Dorbit.Entities.Abstractions;
using Dorbit.Installers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Extensions;

public static class QueryableExtensions
{
    private static IMemoryCache _memoryCache;
    private static IMemoryCache MemoryCache => _memoryCache ??= FrameworkInstaller.ServiceProvider.GetService<IMemoryCache>();
        
    public static T GetById<T>(this IQueryable<T> query, Guid id) where T : IEntity
    {
            
        return query.FirstOrDefault(x => x.Id == id);
    }
    
    public static async Task<List<T>> ToListAsyncWithCache<T>(this IQueryable<T> query, string key, TimeSpan duration)
    {
        key = $"{nameof(ToListAsyncWithCache)}-{nameof(T)}-{key}";
        if (!MemoryCache.TryGetValue(key, out List<T> result))
        {
            result = await query.ToListAsync();
            MemoryCache.Set(key, result, duration);
        }

        return result;
    }

    public static async Task<T> FirstOrDefaultAsyncWithCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, string key, TimeSpan duration)
    {
        key = $"{nameof(FirstOrDefaultAsyncWithCache)}-{nameof(T)}-{key}";
        if (!MemoryCache.TryGetValue(key, out T result))
        {
            result = await query.FirstOrDefaultAsync(predicate);
            if(result is not null) MemoryCache.Set(key, result, duration);
        }

        return result;
    }

    public static Task<T> GetByIdAsyncWithCache<T>(this IQueryable<T> query, Guid id, string key, TimeSpan duration) where T : IEntity
    {
        return query.FirstOrDefaultAsyncWithCache(x => x.Id == id, $"{key}-{id}", duration);
    }
}