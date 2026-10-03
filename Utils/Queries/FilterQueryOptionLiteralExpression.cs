using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionLiteralExpression : FilterQueryOptionExpression
{
    public object Value { get; set; }
    public bool IsConstant { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        if (!IsConstant)
            return QueryIdentifier.RequireMember(Value?.ToString());

        if (parameters is null)
            throw new InvalidOperationException("Filter constants must be passed as parameters.");

        var index = parameters.Count;
        parameters.Add(index.ToString(), Value);
        return "@" + index;
    }
}