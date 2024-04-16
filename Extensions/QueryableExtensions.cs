using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class QueryableExtensions
{
    public static T GetById<T>(this IQueryable<T> query, Guid id) where T : IEntity
    {
        return query.FirstOrDefault(x => x.Id == id);
    }

    public static Task<T> GetByIdAsync<T>(this IQueryable<T> query, Guid id) where T : IEntity
    {
        return query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public static async Task<List<T>> ToListAsyncWithCache<T>(this IQueryable<T> query, string key, TimeSpan duration)
    {
        if (App.MemoryCache.TryGetValue(key, out List<T> result)) return result;
        result = await query.ToListAsync();
        App.MemoryCache.Set(key, result, duration);

        return result;
    }

    public static async Task<T> FirstOrDefaultAsyncWithCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, string key, TimeSpan duration)
    {
        if (App.MemoryCache.TryGetValue(key, out T result)) return result;
        result = await query.FirstOrDefaultAsync(predicate);
        if (result is not null) App.MemoryCache.Set(key, result, duration);

        return result;
    }

    public static Task<T> GetByIdAsyncWithCache<T>(this IQueryable<T> query, Guid id, string key, TimeSpan duration) where T : IEntity
    {
        return query.FirstOrDefaultAsyncWithCache(x => x.Id == id, $"{key}-{id}", duration);
    }

    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, Func<bool> condition, Expression<Func<T, bool>> predicate)
    {
        return (condition() ? query.Where(predicate) : query);
    }
    
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}