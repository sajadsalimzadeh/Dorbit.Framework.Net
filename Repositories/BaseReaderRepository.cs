using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Models;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Repositories;

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