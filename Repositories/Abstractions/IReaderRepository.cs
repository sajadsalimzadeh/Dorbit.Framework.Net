using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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
    Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<PagedListResult<TEntity>> SelectAsync(QueryOptions queryOptions, CancellationToken cancellationToken = default);
    Task<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    Task<TEntity> LastOrDefaultAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

public interface IReaderRepository<TEntity> : IReaderRepository<TEntity, Guid> where TEntity : class, IEntity<Guid>;