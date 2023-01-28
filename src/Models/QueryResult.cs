namespace Devor.Framework.Models
{
    public class QueryResult<T> : OperationResult
    {
        public T Data { get; set; }

        public QueryResult() { }
        public QueryResult(T data) { Data = data; }
    }
}
