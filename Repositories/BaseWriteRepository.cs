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

public class BaseWriteRepository<T> : BaseReadRepository<T>, IWriterRepository<T> where T : class, IEntity
{
    private readonly IDbContext _dbContext;

    public BaseWriteRepository(IDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual Task<T> InsertAsync(T model)
    {
        return _dbContext.InsertEntityAsync(model);
    }

    public virtual Task BulkInsertAsync(Func<T, bool> predicate)
    {
        return BulkInsertAsync(Set().Where(predicate).ToList());
    }

    public virtual Task BulkInsertAsync(List<T> entities)
    {
        return _dbContext.BulkInsertEntityAsync(entities);
    }

    public virtual Task<T> UpdateAsync(T model)
    {
        return _dbContext.UpdateEntityAsync(model);
    }

    public virtual Task BulkUpdateAsync(Func<T, bool> predicate)
    {
        return BulkUpdateAsync(Set().Where(predicate).ToList());
    }

    public virtual Task BulkUpdateAsync(List<T> entities)
    {
        return _dbContext.BulkUpdateEntityAsync(entities);
    }

    public virtual Task<T> DeleteAsync(T model)
    {
        return _dbContext.DeleteEntityAsync(model);
    }

    public virtual Task BulkDeleteAsync(Func<T, bool> predicate)
    {
        return BulkDeleteAsync(Set().Where(predicate).ToList());
    }

    public virtual Task BulkDeleteAsync(List<T> entities)
    {
        return _dbContext.BulkDeleteEntityAsync(entities);
    }

    public virtual Task<T> SaveAsync(T model)
    {
        if (model.Id != Guid.Empty) return _dbContext.UpdateEntityAsync(model);
        return _dbContext.InsertEntityAsync(model);
    }

    //================== Extended Methods ==================\\
    public Task<T> InsertAsync<TR>(TR dto)
    {
        var mapper = _dbContext.ServiceProvider.GetService<IMapper>();
        return InsertAsync(mapper.Map<T>(dto));
    }

    public async Task<T> DeleteAsync(Guid id)
    {
        return await DeleteAsync(await GetByIdAsync(id));
    }

    public async Task<T> UpdateAsync<TR>(Guid id, TR dto)
    {
        return await UpdateAsync(dto.MapTo(await GetByIdAsync(id)));
    }

    public async Task<T> SaveAsync<TR>(Guid id, TR dto)
    {
        return await SaveAsync(dto.MapTo(await GetByIdAsync(id) ?? Activator.CreateInstance<T>()));
    }

    public async Task<T> SaveAsync<TR>(T model, TR dto)
    {
        return await SaveAsync(dto.MapTo(model ?? Activator.CreateInstance<T>()));
    }
}