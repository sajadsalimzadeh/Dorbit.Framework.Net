using Dorbit.Framework.Contracts.Results;

namespace Dorbit.Framework.Contracts;

public class ExceptionResult<T> : QueryResult<T>
{
    public string StackTrace { get; set; }
}