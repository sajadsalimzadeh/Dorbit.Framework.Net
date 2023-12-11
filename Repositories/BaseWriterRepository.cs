using AutoMapper;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Repositories;

public class BaseWriterRepository<T> : BaseReaderRepository<T>, IWriterRepository<T> where T : class, IEntity
{
    private readonly IDbContext dbContext;

    public BaseWriterRepository(IDbContext dbContext) : base(dbContext)
    {
        this.dbContext = dbContext;
    }

    public virtual Task<T> InsertAsync(T model)
    {
        return dbContext.InsertEntityAsync(model);
    }

    public virtual Task<T> RemoveAsync(T model)
    {
        return dbContext.RemoveEntityAsync(model);
    }

    public virtual Task<T> UpdateAsync(T model)
    {
        return dbContext.UpdateEntityAsync(model);
    }

    public virtual Task<T> SaveAsync(T model)
    {
        if(model.Id != Guid.Empty) return dbContext.UpdateEntityAsync(model);
        return dbContext.InsertEntityAsync(model);
    }
    //================== Extended Methods ==================\\
    public Task<T> InsertAsync<TR>(TR dto)
    {
        var mapper = dbContext.ServiceProvider.GetService<IMapper>();
        return InsertAsync(mapper.Map<T>(dto));
    }

    public async Task<T> RemoveAsync(Guid id)
    {
        return await RemoveAsync(await GetByIdAsync(id));
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