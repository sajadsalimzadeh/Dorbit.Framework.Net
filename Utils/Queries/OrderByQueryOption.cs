using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Utils.Queries;

public class OrderByQueryOption
{
    public List<KeyValuePair<string, bool>> Items { get; set; }

    public string ToSql()
    {
        if (Items is null || Items.Count == 0) return null;
        if (Items.Count > 8) throw new OperationException(FrameworkErrors.QueryIsTooComplex);

        return string.Join(",",
            Items.ConvertAll(x => $"{QueryIdentifier.RequireMember(x.Key)} {(x.Value ? "DESC" : "ASC")}"));
    }

    public OrderByQueryOption Clone()
    {
        return new OrderByQueryOption()
        {
            Items = Items?.ToList()
        };
    }
}