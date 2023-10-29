using Dorbit.Entities.Abstractions;
using Dorbit.Models;
using Dorbit.Utils.Queries;

namespace Dorbit.Repositories.Abstractions
{
    public interface IReaderRepository<T> where T : class, IEntity
    {
        T GetById(Guid id);
        IQueryable<T> Set(bool excludeDeleted = true);
        IEnumerable<T> GetAll();
        PagedListResult<T> Select(QueryOptions queryOptions);
        T First();
        T Last();
        int Count();
    }
}
