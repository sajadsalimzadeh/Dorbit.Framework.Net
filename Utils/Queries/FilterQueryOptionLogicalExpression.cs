namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionLogicalExpression : FilterQueryOptionExpression
{
    public FilterQueryOptionExpression Left { get; set; }
    public FilterQueryOptionLogicalOperators Operator { get; set; }
    public FilterQueryOptionExpression Right { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        return $"({Left.ToSql(parameters)} {Operator.GetSqlValue()} {Right.ToSql(parameters)})";
    }
}