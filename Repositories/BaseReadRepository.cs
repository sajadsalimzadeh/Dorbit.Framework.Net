using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;
using Dorbit.Framework.Utils.Queries;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Repositories;

public class BaseReadRepository<TEntity, TKey>(IDbContext dbContext) : IReaderRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    public IDbContext DbContext => dbContext;

    protected IServiceProvider ServiceProvider => dbContext.ServiceProvider;

    public virtual IQueryable<TEntity> Set(bool excludeDeleted = true)
    {
        return dbContext.DbSet<TEntity, TKey>(excludeDeleted);
    }

    public virtual Task<int> CountAsync()
    {
        return Set().CountAsync();
    }

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Set().CountAsync(predicate);
    }

    public virtual Task<List<TEntity>> GetAllAsync()
    {
        return Set().ToListAsync();
    }

    public virtual Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Set().Where(predicate).ToListAsync();
    }

    public virtual Task<TEntity> GetByIdAsync(TKey id)
    {
        return Set().FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public Task<TEntity> FirstOrDefaultAsync()
    {
        return Set().FirstOrDefaultAsync();
    }

    public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Set().FirstOrDefaultAsync(predicate);
    }

    public Task<TEntity> LastOrDefaultAsync()
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    }

    public Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync(predicate);
    }

    public virtual Task<PagedListResult<TEntity>> SelectAsync(QueryOptions queryOptions)
    {
        return Set().ApplyToPagedListAsync(queryOptions);
    }
}

public class BaseReadRepository<TEntity>(IDbContext dbContext) : BaseReadRepository<TEntity, Guid>(dbContext)
    where TEntity : class, IEntity<Guid>;