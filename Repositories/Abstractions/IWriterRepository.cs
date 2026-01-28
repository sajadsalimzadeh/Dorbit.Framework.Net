using System;
using System.Collections.Generic;
using System.Text.Json;
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
    Task<TEntity> UpdateAsync<TPatch>(TKey key, TPatch patch, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync<TPatch>(TEntity entity, TPatch patch, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateWithJsonAsync<TPatch>(TKey key, JsonElement patch, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateWithJsonAsync<TPatch>(TEntity entity, JsonElement patch, CancellationToken cancellationToken = default);
    Task<TEntity> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}

public interface IWriterRepository<TEntity> : IWriterRepository<TEntity, Guid> where TEntity : class, IEntity<Guid>;