using Dorbit.Entities.Abstractions;

namespace Dorbit.Repositories
{
    public static class Extensions
    {
        public static T GetById<T>(this IEnumerable<T> query, long id) where T : IEntity
        {
            return query.FirstOrDefault(x => x.Id == id);
        }
    }
}
