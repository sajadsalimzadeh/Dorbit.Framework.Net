using System.Linq.Expressions;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Models;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Repositories;

public class BaseReadRepository<T> : IReaderRepository<T> where T : class, IEntity
{
    private readonly IDbContext _dbContext;
    public IDbContext DbContext => _dbContext;

    protected IServiceProvider ServiceProvider => _dbContext.ServiceProvider;

    public BaseReadRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual IQueryable<T> Set(bool excludeDeleted = true)
    {
        return _dbContext.DbSet<T>(excludeDeleted);
    }

    public virtual Task<int> CountAsync()
    {
        return Set().CountAsync();
    }

    public virtual Task<List<T>> GetAllAsync()
    {
        return Set().ToListAsync();
    }

    public virtual Task<T> GetByIdAsync(Guid id)
    {
        return Set().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<T> FirstOrDefaultAsync()
    {
        return Set().FirstOrDefaultAsync();
    }

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return Set().FirstOrDefaultAsync(predicate);
    }

    public Task<T> LastOrDefaultAsync()
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    }

    public Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync(predicate);
    }

    public virtual Task<PagedListResult<T>> SelectAsync(QueryOptions queryOptions)
    {
        return Set().ApplyToPagedListAsync(queryOptions);
    }
}