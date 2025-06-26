using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Repositories;

public class BaseWriteRepository<TEntity, TKey>(IDbContext dbContext) : BaseReadRepository<TEntity, TKey>(dbContext), IWriterRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly IDbContext _dbContext = dbContext;

    public virtual Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _dbContext.InsertEntityAsync<TEntity, TKey>(entity, cancellationToken);
    }

    public virtual Task BulkInsertAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
    {
        var entities = Set().AsEnumerable().Where(predicate).ToList();
        return BulkInsertAsync(entities, cancellationToken);
    }

    public virtual Task BulkInsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _dbContext.BulkInsertEntityAsync<TEntity, TKey>(entities, cancellationToken);
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _dbContext.UpdateEntityAsync<TEntity, TKey>(entity, cancellationToken);
    }

    public virtual Task BulkUpdateAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
    {
        return BulkUpdateAsync(Set().AsEnumerable().Where(predicate).ToList(), cancellationToken);
    }

    public virtual Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _dbContext.BulkUpdateEntityAsync<TEntity, TKey>(entities, cancellationToken);
    }

    public virtual Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _dbContext.DeleteEntityAsync<TEntity, TKey>(entity, cancellationToken);
    }

    public virtual Task BulkDeleteAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
    {
        var entities = Set().AsEnumerable().Where(predicate).ToList();
        return BulkDeleteAsync(entities, cancellationToken);
    }

    public virtual Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _dbContext.BulkDeleteEntityAsync<TEntity, TKey>(entities, cancellationToken);
    }

    public virtual Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id is Guid guid)
        {
            return guid != Guid.Empty ? UpdateAsync(entity, cancellationToken) : InsertAsync(entity, cancellationToken);
        }

        if (entity.Id.GetType().IsNumeric())
        {
            var longValue = Convert.ToInt64(entity.Id);
            return longValue > 0 ? UpdateAsync(entity, cancellationToken) : InsertAsync(entity, cancellationToken);
        }

        return InsertAsync(entity, cancellationToken);
    }

    //================== Extended Methods ==================\\
    public Task<TEntity> InsertAsync<TDto>(TDto dto, CancellationToken cancellationToken = default)
    {
        var mapper = _dbContext.ServiceProvider.GetService<IMapper>();
        return InsertAsync(mapper.Map<TEntity>(dto), cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return await DeleteAsync(entity, cancellationToken);
    }

    public async Task<TEntity> UpdateAsync<TDto>(TKey id, TDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return await UpdateAsync(dto.MapTo(entity), cancellationToken);
    }

    public async Task<TEntity> PatchAsync(TKey key, object patch, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(key, cancellationToken);
        entity = entity.PatchObject(patch);
        return await UpdateAsync(entity, cancellationToken);
    }

    public async Task<TEntity> SaveAsync<TDto>(TKey id, TDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return await SaveAsync(entity, dto, cancellationToken);
    }

    public async Task<TEntity> SaveAsync<TDto>(TEntity entity, TDto dto, CancellationToken cancellationToken = default)
    {
        if (entity is not null)
        {
            var id = entity.Id;
            entity = dto.MapTo(Activator.CreateInstance<TEntity>());
            entity.Id = id;
        }
        else
        {
            entity = dto.MapTo(Activator.CreateInstance<TEntity>());
        }

        return await SaveAsync(entity, cancellationToken);
    }
}

public class BaseWriteRepository<TEntity>(IDbContext dbContext) : BaseWriteRepository<TEntity, Guid>(dbContext)
    where TEntity : class, IEntity<Guid>;