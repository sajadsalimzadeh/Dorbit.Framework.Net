using Dorbit.Framework.Models;

namespace Dorbit.Framework.Extensions;

public static class CommandResultExtensions
{
    public static CommandResult ToCommandResult(this bool success)
    {
        return new CommandResult(success);
    }

    public static QueryResult<T> ToQueryResult<T>(this T data)
    {
        return new QueryResult<T>(data) { Success = true };
    }

    public static async Task<QueryResult<T>> ToQueryResultAsync<T>(this Task<T> dataTask)
    {
        return new QueryResult<T>(await dataTask) { Success = true };
    }
}