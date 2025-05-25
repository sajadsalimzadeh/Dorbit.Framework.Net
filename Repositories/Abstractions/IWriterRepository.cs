using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IWriterRepository<TEntity, TKey> : IReaderRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity> InsertAsync(TEntity entity);
    Task BulkInsertAsync(List<TEntity> entities);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task BulkUpdateAsync(List<TEntity> entities);
    Task<TEntity> DeleteAsync(TEntity entity);
    Task BulkDeleteAsync(List<TEntity> entities);

    Task<TEntity> InsertAsync<TR>(TR dto);
    Task<TEntity> UpdateAsync<TR>(TKey id, TR dto);
    Task<TEntity> PatchAsync(TKey key, object patch);
    Task<TEntity> PatchAsync(TEntity entity, object patch);
    Task<TEntity> DeleteAsync(TKey id);
}

public interface IWriterRepository<TEntity> : IWriterRepository<TEntity, Guid> where TEntity : class, IEntity<Guid>;