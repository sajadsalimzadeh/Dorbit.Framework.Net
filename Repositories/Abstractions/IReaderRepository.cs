using Dorbit.Entities.Abstractions;
using Dorbit.Models;
using Dorbit.Utils.Queries;

namespace Dorbit.Repositories.Abstractions
{
    public interface IReaderRepository<T> where T : class, IEntity
    {
        T GetById(long id);
        T GetByIdWithCache(long id, TimeSpan? timeSpan = null);
        IQueryable<T> Set(bool excludeDeleted = true);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllWithCache(TimeSpan? timeSpan = null);
        PagedListResult<T> Select(QueryOptions queryOptions);
        T First();
        T Last();
        int Count();
    }
}
