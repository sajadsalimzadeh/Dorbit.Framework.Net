namespace Dorbit.Models
{
    public static class Extensions
    {
        public static QueryResult<T> ToQueryResult<T>(this T obj)
        {
            return new QueryResult<T>()
            {
                Data = obj,
                Success = true,
            };
        }
    }
}
