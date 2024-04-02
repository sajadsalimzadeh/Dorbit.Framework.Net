using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Utils.Queries;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IReaderRepository<T> where T : class, IEntity
{
    IDbContext DbContext { get; }

    IQueryable<T> Set(bool excludeDeleted = true);
    Task<T> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<PagedListResult<T>> SelectAsync(QueryOptions queryOptions);
    Task<T> FirstOrDefaultAsync();
    Task<T> LastOrDefaultAsync();
    Task<int> CountAsync();
}