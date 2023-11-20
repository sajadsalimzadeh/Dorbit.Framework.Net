using Dorbit.Entities.Abstractions;
using Dorbit.Models;
using Dorbit.Utils.Queries;

namespace Dorbit.Repositories.Abstractions;

public interface IReaderRepository<T> where T : class, IEntity
{
    IQueryable<T> Set(bool excludeDeleted = true);
    Task<T> GetByIdAsync(Guid id);
    Task<List<T>> GetAll();
    Task<PagedListResult<T>> Select(QueryOptions queryOptions);
    Task<T> First();
    Task<T> Last();
    Task<int> CountAsync();
}