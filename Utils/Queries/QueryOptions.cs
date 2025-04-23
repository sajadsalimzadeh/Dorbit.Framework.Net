using System.Linq;
using System.Linq.Dynamic.Core;

namespace Dorbit.Framework.Utils.Queries;

public class QueryOptions
{
    public FilterQueryOption Filters { get; protected set; } = new();
    public OrderByQueryOption OrderBy { get; protected set; } = new();
    public TopQueryOption Top { get; protected set; } = new();
    public SkipQueryOption Skip { get; protected set; } = new();

    public IQueryable<T> ApplyTo<T>(IQueryable<T> query)
    {
        var filterQuery = Filters.ToSql();
        var orderQuery = OrderBy.ToSql();

        if (filterQuery?.Length > 0) query = query.Where(filterQuery);
        if (orderQuery?.Length > 0) query = query.OrderBy(orderQuery);

        if (Skip.Value > 0)
            query = query.Skip(Skip.Value);

        if (Top.Value > 0)
            query = query.Take(Top.Value);

        return query;
    }

    public IQueryable<T> ApplyCountTo<T>(IQueryable<T> query)
    {
        var filterQuery = Filters.ToSql();
        if (filterQuery?.Length > 0) query = query.Where(filterQuery);
        return query;
    }

    public QueryOptions Clone()
    {
        return new QueryOptions()
        {
            Filters = Filters.Clone(),
            OrderBy = OrderBy.Clone(),
            Skip = Skip.Clone(),
            Top = Top.Clone(),
        };
    }
}