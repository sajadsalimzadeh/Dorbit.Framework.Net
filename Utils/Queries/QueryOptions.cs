using System.Linq;
using System.Linq.Dynamic.Core;

namespace Dorbit.Framework.Utils.Queries;

public class QueryOptions
{
    public FilterQueryOption Filters { get; protected set; }
    public OrderByQueryOption OrderBy { get; protected set; }
    public TopQueryOption Top { get; protected set; }
    public SkipQueryOption Skip { get; protected set; }
    public QueryOptionsSwitches Switches { get; set; }
    public QueryOptionsDefaults Defaults { get; set; }
    public QueryOptionsPatches Patches { get; set; }

    public QueryOptions()
    {
        Filters = new FilterQueryOption();
        OrderBy = new OrderByQueryOption();
        Top = new TopQueryOption();
        Skip = new SkipQueryOption();
        Switches = new QueryOptionsSwitches();
        Defaults = new QueryOptionsDefaults();
        Patches = new QueryOptionsPatches();
    }

    public IQueryable<T> ApplyTo<T>(IQueryable<T> query)
    {
        var filterQuery = Filters.ToSql();
        var orderQuery = OrderBy.ToSql();
        if (Switches.EnableFilter && filterQuery?.Length > 0) query = query.Where(filterQuery);
        if (Switches.EnableOrderBy && orderQuery?.Length > 0) query = query.OrderBy(orderQuery);
            
        if (Switches.EnableSkip && Skip.Value > 0) 
            query = query.Skip(Skip.Value);
        else
            query = query.Skip(Defaults.PageIndex * Defaults.PageSize);

        if (Switches.EnableTop && Top.Value > 0)
            query = query.Take(Top.Value);
        else
            query = query.Take(Defaults.PageSize);
            
        return query;
    }

    public IQueryable<T> ApplyCountTo<T>(IQueryable<T> query)
    {
        var filterQuery = Filters.ToSql();
        if (Switches.EnableFilter && filterQuery?.Length > 0) query = query.Where(filterQuery);
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