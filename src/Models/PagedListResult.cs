using Devor.Framework.Utils.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Devor.Framework.Models
{
    public class PagedListResult<T> : QueryResult<IEnumerable<T>>
    {
        public int TotalCount { get; set; }

        public PagedListResult<TR> Select<TR>(Func<T, TR> func)
        {
            return new PagedListResult<TR>()
            {
                TotalCount = TotalCount,
                Data = Data.Select(func).ToList()
            };
        }

        public PagedListResult<T> ApplyToList(QueryOptions queryOptions)
        {
            return new PagedListResult<T>()
            {
                Data = queryOptions.ApplyTo(Data.AsQueryable()),
                TotalCount = queryOptions.ApplyCountTo(Data.AsQueryable()).Count()
            };
        }

        public QueryResult<IEnumerable<T>> ToOperationResult()
        {
            return new QueryResult<IEnumerable<T>>()
            {
                Success = true,
                Data = Data,
            };
        }
    }
}
