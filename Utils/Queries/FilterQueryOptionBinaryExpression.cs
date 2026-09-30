using System.Collections.Generic;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionBinaryExpression : FilterQueryOptionExpression
{
    public FilterQueryOptionExpression Left { get; set; }
    public FilterQueryOptionBinaryOperators Operator { get; set; }
    public FilterQueryOptionExpression Right { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        if (Left is null || Right is null)
            throw new OperationException(FrameworkErrors.QueryIsInvalid);

        var format = Operator.GetFormat();
        if (string.IsNullOrEmpty(format)) format = "{0} {1} {2}";

        return string.Format($"({format})", Left.ToSql(parameters), Operator.GetSqlValue(), Right.ToSql(parameters));
    }
}