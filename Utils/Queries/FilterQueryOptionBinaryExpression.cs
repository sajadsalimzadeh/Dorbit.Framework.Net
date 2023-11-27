namespace Dorbit.Framework.Utils.Queries;

public class FilterQueryOptionBinaryExpression : FilterQueryOptionExpression
{
    public FilterQueryOptionExpression Left { get; set; }
    public FilterQueryOptionBinaryOperators Operator { get; set; }
    public FilterQueryOptionExpression Right { get; set; }

    public override string ToSql(Dictionary<string, object> parameters)
    {
        var format = Operator.GetFormat();
        if (string.IsNullOrEmpty(format)) format = "{0} {1} {2}";

        if (Left is FilterQueryOptionLiteralExpression literalLeft && 
            Right is FilterQueryOptionLiteralExpression literalRight && parameters != null)
        {
            var key = literalLeft.Value.ToString();
            int i = 0;
            while (parameters.ContainsKey(key + i)) i++;
            parameters.Add(key + i, literalRight.Value);
            return string.Format($"({format})", key, Operator.GetSqlValue(), "@" + key + i);
        }
        else return string.Format($"({format})", Left.ToSql(parameters), Operator.GetSqlValue(), Right.ToSql(parameters));
    }
}