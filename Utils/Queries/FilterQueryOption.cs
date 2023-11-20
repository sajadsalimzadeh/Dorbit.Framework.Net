namespace Dorbit.Utils.Queries;

public class FilterQueryOption
{
    public FilterQueryOptionExpression Expression { get; set; }
    public string ToSql()
    {
        return Expression?.ToSql(null);
    }

    public string ToSql(out Dictionary<string, object> parameters)
    {
        var dict = new Dictionary<string, object>();
        var sql =  Expression?.ToSql(dict);
        parameters = dict;
        return sql;
    }

    public FilterQueryOption Clone()
    {
        return new FilterQueryOption()
        {
            Expression = Expression
        };
    }
}