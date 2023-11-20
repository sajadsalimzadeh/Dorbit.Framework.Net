using AutoMapper;
using Dorbit.Database.Abstractions;
using Dorbit.Entities.Abstractions;
using Dorbit.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Repositories;

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
        var mapper = dbContext.ServiceProvider.GetService<IMapper>();
        return await UpdateAsync(mapper.Map(dto, await GetByIdAsync(id)));
    }
}