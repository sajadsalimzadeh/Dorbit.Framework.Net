using Dorbit.Models;

namespace Dorbit.Extensions;

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

    public static Task<QueryResult<T>> ToQueryResultAsync<T>(this T data)
    {
        return Task.FromResult(new QueryResult<T>(data) { Success = true });
    }
}