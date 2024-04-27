using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Utils.Queries;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IReaderRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    IDbContext DbContext { get; }

    IQueryable<TEntity> Set(bool excludeDeleted = true);
    Task<TEntity> GetByIdAsync(TKey id);
    Task<List<TEntity>> GetAllAsync();
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
    Task<PagedListResult<TEntity>> SelectAsync(QueryOptions queryOptions);
    Task<TEntity> FirstOrDefaultAsync();
    Task<TEntity> LastOrDefaultAsync();
    Task<int> CountAsync();
}

public interface IReaderRepository<TEntity> : IReaderRepository<TEntity, Guid>  where TEntity : class, IEntity<Guid>;