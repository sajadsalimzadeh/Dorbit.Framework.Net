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
    
    public QueryResult(bool success)
    {
        Success = success;
    }
}

public static class QueryResultExtensions
{
    public static async Task<T> ToDataAsync<T>(this Task<QueryResult<T>> queryResultTask)
    {
        var result = (await queryResultTask);
        return result is not null ? result.Data : default;
    }
}