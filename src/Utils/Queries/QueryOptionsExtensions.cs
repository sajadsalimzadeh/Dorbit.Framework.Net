using Devor.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devor.Framework.Utils.Queries
{
    public static class QueryOptionsExtensions
    {
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

        public static IEnumerable<T> ApplyToList<T>(this IQueryable<T> query, QueryOptions queryOptions)
        {
            return queryOptions.ApplyTo(query);
        }

        public static PagedListResult<T> ApplyToPagedList<T>(this IQueryable<T> query, QueryOptions queryOptions)
        {
            var itemsQuery = query.AsQueryable();
            var countQuery = query.AsQueryable();
            return new PagedListResult<T>()
            {
                Data = queryOptions.ApplyTo(itemsQuery).ToList(),
                TotalCount = queryOptions.ApplyCountTo(countQuery).Count()
            };
        }

        public static IEnumerable<T> ApplyToList<T>(this IEnumerable<T> enumerable, QueryOptions queryOptions)
        {
            return enumerable.AsQueryable().ApplyToList(queryOptions);
        }

        public static PagedListResult<T> ApplyToPagedList<T>(this IEnumerable<T> enumerable, QueryOptions queryOptions)
        {
            return new PagedListResult<T>()
            {
                Data = enumerable.ApplyToList(queryOptions),
                TotalCount = enumerable.Count()
            };
        }
    }
}
