using System.Collections.Generic;

namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionGroupExpression : FilterQueryOptionExpression
{
    public FilterQueryOptionExpression Expression { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        return "(" + Expression.ToSql(parameters) + ")";
    }
}