using System.Collections.Generic;

namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionUnaryExpression : FilterQueryOptionExpression
{
    public FilterQueryOptionUnaryOperators Operator { get; set; }
    public FilterQueryOptionExpression Expression { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        return $"({Operator.GetSqlValue()} {Expression.ToSql(parameters)})";
    }
}