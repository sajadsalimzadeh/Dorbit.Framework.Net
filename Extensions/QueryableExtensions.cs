using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Dorbit.Framework.Extensions;

public static class QueryableExtensions
{
    public static TEntity GetById<TEntity, TKey>(this IQueryable<TEntity> query, TKey id) where TEntity : IEntity<TKey>
    {
        return query.FirstOrDefault(x => x.Id.Equals(id));
    }

    public static TEntity GetById<TEntity>(this IQueryable<TEntity> query, Guid id) where TEntity : IEntity
    {
        return query.GetById<TEntity, Guid>(id);
    }

    public static Task<TEntity> GetByIdAsync<TEntity>(this IQueryable<TEntity> query, Guid id) where TEntity : IEntity
    {
        return query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public static Task<List<TEntity>> ToListAsyncBy<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return query.Where(predicate).ToListAsync(cancellationToken: cancellationToken);
    }

    public static async Task<List<TEntity>> ToListAsyncWithCache<TEntity>(this IQueryable<TEntity> query, string key, TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        if (App.MemoryCache.TryGetValue(key, out List<TEntity> result)) return result;
        result = await query.ToListAsync(cancellationToken: cancellationToken);
        App.MemoryCache.Set(key, result, duration);

        return result;
    }

    public static async Task<TEntity> FirstOrDefaultAsyncWithCache<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate,
        string key,
        TimeSpan duration, CancellationToken cancellationToken = default)
    {
        if (App.MemoryCache.TryGetValue(key, out TEntity result)) return result;
        result = await query.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        if (result is not null) App.MemoryCache.Set(key, result, duration);

        return result;
    }


    public static Task<TEntity> FirstOrDefaultAsyncWithCache<TEntity>(this IQueryable<TEntity> query, string key, TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        return query.FirstOrDefaultAsyncWithCache(x => true, key, duration, cancellationToken);
    }

    public static Task<TEntity> GetByIdAsyncWithCache<TEntity>(this IQueryable<TEntity> query, Guid id, string key, TimeSpan duration,
        CancellationToken cancellationToken = default) where TEntity : IEntity
    {
        return query.FirstOrDefaultAsyncWithCache(x => x.Id == id, $"{key}-{id}", duration, cancellationToken: cancellationToken);
    }

    public static IQueryable<TEntity> WhereIf<TEntity>(this IQueryable<TEntity> query, Func<bool> condition,
        Expression<Func<TEntity, bool>> predicate)
    {
        return (condition() ? query.Where(predicate) : query);
    }

    public static IQueryable<TEntity> WhereIf<TEntity>(this IQueryable<TEntity> query, bool condition, Expression<Func<TEntity, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}