using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IWriterRepository<TEntity, TKey> : IReaderRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task BulkInsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> InsertAsync<TR>(TR dto, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync<TR>(TKey id, TR dto, CancellationToken cancellationToken = default);
    Task<TEntity> PatchAsync(TKey key, object patch, CancellationToken cancellationToken = default);
    Task<TEntity> PatchAsync(TEntity entity, object patch, CancellationToken cancellationToken = default);
    Task<TEntity> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}

public interface IWriterRepository<TEntity> : IWriterRepository<TEntity, Guid> where TEntity : class, IEntity<Guid>;