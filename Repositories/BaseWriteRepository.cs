using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Repositories;

public class BaseWriteRepository<TEntity, TKey> : BaseReadRepository<TEntity, TKey>, IWriterRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    private readonly IDbContext _dbContext;

    public BaseWriteRepository(IDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual Task<TEntity> InsertAsync(TEntity entity)
    {
        return _dbContext.InsertEntityAsync<TEntity, TKey>(entity);
    }

    public virtual Task BulkInsertAsync(Func<TEntity, bool> predicate)
    {
        return BulkInsertAsync(Set().Where(predicate).ToList());
    }

    public virtual Task BulkInsertAsync(List<TEntity> entities)
    {
        return _dbContext.BulkInsertEntityAsync<TEntity, TKey>(entities);
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity)
    {
        return _dbContext.UpdateEntityAsync<TEntity, TKey>(entity);
    }

    public virtual Task BulkUpdateAsync(Func<TEntity, bool> predicate)
    {
        return BulkUpdateAsync(Set().AsEnumerable().Where(predicate).ToList());
    }

    public virtual Task BulkUpdateAsync(List<TEntity> entities)
    {
        return _dbContext.BulkUpdateEntityAsync<TEntity, TKey>(entities);
    }

    public virtual Task<TEntity> DeleteAsync(TEntity entity)
    {
        return _dbContext.DeleteEntityAsync<TEntity, TKey>(entity);
    }

    public virtual Task BulkDeleteAsync(Func<TEntity, bool> predicate)
    {
        return BulkDeleteAsync(Set().AsEnumerable().Where(predicate).ToList());
    }

    public virtual Task BulkDeleteAsync(List<TEntity> entities)
    {
        return _dbContext.BulkDeleteEntityAsync<TEntity, TKey>(entities);
    }

    public virtual Task<TEntity> SaveAsync(TEntity model)
    {
        if (model.Id is Guid guid)
        {
            return guid != Guid.Empty ? UpdateAsync(model) : InsertAsync(model);
        }
        
        if (model.Id.IsNumericType())
        {
            var longValue = Convert.ToInt64(model.Id);
            return longValue > 0 ? UpdateAsync(model) : InsertAsync(model);
        }

        return InsertAsync(model);
    }

    //================== Extended Methods ==================\\
    public Task<TEntity> InsertAsync<TR>(TR dto)
    {
        var mapper = _dbContext.ServiceProvider.GetService<IMapper>();
        return InsertAsync(mapper.Map<TEntity>(dto));
    }

    public async Task<TEntity> DeleteAsync(TKey id)
    {
        return await DeleteAsync(await GetByIdAsync(id));
    }

    public async Task<TEntity> UpdateAsync<TR>(TKey id, TR dto)
    {
        return await UpdateAsync(dto.MapTo(await GetByIdAsync(id)));
    }

    public async Task<TEntity> SaveAsync<TR>(TKey id, TR dto)
    {
        return await SaveAsync(dto.MapTo(await GetByIdAsync(id) ?? Activator.CreateInstance<TEntity>()));
    }

    public async Task<TEntity> SaveAsync<TR>(TEntity model, TR dto)
    {
        return await SaveAsync(dto.MapTo(model ?? Activator.CreateInstance<TEntity>()));
    }
}

public class BaseWriteRepository<TEntity> : BaseWriteRepository<TEntity, Guid>  where TEntity : class, IEntity<Guid>
{
    public BaseWriteRepository(IDbContext dbContext) : base(dbContext)
    {
    }
} 