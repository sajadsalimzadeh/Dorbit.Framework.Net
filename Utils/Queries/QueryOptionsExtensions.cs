using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Utils.Queries;

public static class QueryOptionsExtensions
{
    public static IQueryable<T> Apply<T>(this IQueryable<T> query, ODataQueryOptions<T> queryOptions)
    {
        if (queryOptions.SelectExpand is not null)
        {
            var tempQuery = queryOptions.ApplyTo(query);
            query = query.Select(x => tempQuery.Expression) as IQueryable<T>;
        }
        if (queryOptions.Filter is not null) query = queryOptions.Filter.ApplyTo(query, new ODataQuerySettings()) as IQueryable<T> ?? query;
        if (queryOptions.OrderBy is not null) query = queryOptions.OrderBy.ApplyTo(query, new ODataQuerySettings()) ?? query;
        if (queryOptions.Skip is not null) query = queryOptions.Skip.ApplyTo(query, new ODataQuerySettings()) ?? query;
        if (queryOptions.Top is not null) query = queryOptions.Top.ApplyTo(query, new ODataQuerySettings()) ?? query;
        return query;
    }

    public static IQueryable<T> Apply<T>(this IQueryable<T> query, QueryOptions queryOptions)
    {
        return queryOptions.ApplyTo(query);
    }

    public static IQueryable<T> ApplyCount<T>(this IQueryable<T> query, QueryOptions queryOptions)
    {
        var top = queryOptions.Top.Value;
        var skip = queryOptions.Skip.Value;
        try
        {
            queryOptions.Top.Value = int.MaxValue;
            queryOptions.Skip.Value = 0;
            return queryOptions.ApplyTo(query);
        }
        finally
        {
            queryOptions.Top.Value = top;
            queryOptions.Skip.Value = skip;
        }
    }

    public static IQueryable<T> Apply<T>(this IQueryable<T> query,
        QueryOptions queryOptions,
        Action<QueryOptionsSwitches> switches,
        Action<QueryOptionsDefaults> defaults)
    {
        defaults?.Invoke(queryOptions.Defaults);
        switches?.Invoke(queryOptions.Switches);
        return queryOptions.ApplyTo(query);
    }

    public static IQueryable<T> ApplyToList<T>(this IQueryable<T> query, QueryOptions queryOptions)
    {
        return queryOptions.ApplyTo(query);
    }

    public static async Task<PagedListResult<T>> ApplyToPagedListAsync<T>(this IQueryable<T> query, QueryOptions queryOptions)
    {
        var itemsQuery = query.AsQueryable();
        var countQuery = query.AsQueryable();
        return new PagedListResult<T>()
        {
            Data = await queryOptions.ApplyTo(itemsQuery).ToListAsync(),
            TotalCount = await queryOptions.ApplyCountTo(countQuery).CountAsync()
        };
    }

    public static async Task<PagedListResult<T>> ApplyToPagedListAsync<T>(this IQueryable<T> query, ODataQueryOptions<T> queryOptions)
    {
        var itemsQuery = query.AsQueryable();
        var countQuery = query.AsQueryable();
        var result = new PagedListResult<T>();
        result.Data = await itemsQuery.Apply(queryOptions).ToListAsync();
        if (queryOptions.Filter is not null)
        {
            countQuery = queryOptions.Filter.ApplyTo(countQuery, new ODataQuerySettings()).Cast<T>();
        }

        result.TotalCount = await countQuery.CountAsync();
        return result;
    }

    public static IEnumerable<T> ApplyToList<T>(this IEnumerable<T> enumerable, QueryOptions queryOptions)
    {
        return enumerable.AsQueryable().ApplyToList(queryOptions);
    }
}