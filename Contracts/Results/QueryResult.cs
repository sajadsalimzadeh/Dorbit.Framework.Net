using System.Threading.Tasks;

namespace Dorbit.Framework.Contracts.Results;



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

public static class QueryResultExtensions
{
    public static async Task<T> ToDataAsync<T>(this Task<QueryResult<T>> queryResultTask)
    {
        return (await queryResultTask).Data;
    }
}