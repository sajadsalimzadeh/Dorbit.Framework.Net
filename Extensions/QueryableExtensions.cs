using Dorbit.Entities.Abstractions;

namespace Dorbit.Repositories
{
    public static class QueryableExtensions
    {
        public static T GetById<T>(this IQueryable<T> query, Guid id) where T : IEntity
        {
            return query.FirstOrDefault(x => x.Id == id);
        }
    }
}
