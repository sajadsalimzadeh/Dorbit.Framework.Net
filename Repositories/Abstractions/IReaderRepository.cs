using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Models;
using Dorbit.Framework.Utils.Queries;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IReaderRepository<T> where T : class, IEntity
{
    IQueryable<T> Set(bool excludeDeleted = true);
    Task<T> GetByIdAsync(Guid id);
    Task<List<T>> GetAll();
    Task<PagedListResult<T>> SelectAsync(QueryOptions queryOptions);
    Task<T> FirstOrDefaultAsync();
    Task<T> LastOrDefaultAsync();
    Task<int> CountAsync();
}