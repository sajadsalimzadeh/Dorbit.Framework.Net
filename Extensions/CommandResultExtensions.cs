using Dorbit.Models;

namespace Dorbit.Extensions;

public static class CommandResultExtensions
{
    public static CommandResult ToOperationResult(this bool success)
    {
        return new CommandResult(success);
    }

    public static QueryResult<T> ToQueryResult<T>(this T data)
    {
        return new QueryResult<T>(data) { Success = true };
    }
}