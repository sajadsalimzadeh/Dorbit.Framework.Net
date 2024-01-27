namespace Dorbit.Framework.Contracts;

public class QueryResult<T> : CommandResult
{
    public T Data { get; set; }

    public QueryResult()
    {
    }

    public QueryResult(T data)
    {
        Data = data;
    }
}