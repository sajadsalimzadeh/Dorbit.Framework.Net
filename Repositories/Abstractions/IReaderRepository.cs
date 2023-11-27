using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Models;
using Dorbit.Framework.Utils.Queries;

namespace Dorbit.Framework.Repositories.Abstractions;

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