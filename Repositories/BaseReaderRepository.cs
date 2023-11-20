using Dorbit.Database.Abstractions;
using Dorbit.Entities.Abstractions;
using Dorbit.Models;
using Dorbit.Repositories.Abstractions;
using Dorbit.Utils.Queries;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Repositories;

public class BaseReaderRepository<T> : IReaderRepository<T> where T : class, IEntity
{
    private readonly IDbContext dbContext;

    protected IServiceProvider ServiceProvider => dbContext.ServiceProvider;

    public BaseReaderRepository(IDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public virtual IQueryable<T> Set(bool excludeDeleted = true)
    {
        return dbContext.DbSet<T>(excludeDeleted);
    }

    public virtual Task<int> CountAsync()
    {
        return Set().CountAsync();
    }

    public virtual Task<List<T>> GetAll()
    {
        return Set().ToListAsync();
    }

    public virtual Task<T> GetByIdAsync(Guid id)
    {
        return Set().FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual Task<T> First()
    {
        return Set().FirstOrDefaultAsync();
    }

    public virtual Task<T> Last()
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    }

    public virtual Task<PagedListResult<T>> Select(QueryOptions queryOptions)
    {
        return Set().ApplyToPagedListAsync(queryOptions);
    }
}