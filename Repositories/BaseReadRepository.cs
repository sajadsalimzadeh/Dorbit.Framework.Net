using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Extensions;
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

    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Set().CountAsync(cancellationToken: cancellationToken);
    }

    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Set().CountAsync(predicate, cancellationToken: cancellationToken);
    }

    public virtual Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Set().ToListAsync(cancellationToken: cancellationToken);
    }

    public virtual Task<List<TEntity>> GetAllAsyncWithCache(TimeSpan timeToLive)
    {
        return Set().ToListAsyncWithCache($"[{typeof(TEntity).Name}]-{nameof(GetAllAsyncWithCache)}", timeToLive);
    }

    public virtual Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Set().Where(predicate).ToListAsync(cancellationToken: cancellationToken);
    }

    public virtual Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return Set().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken);
    }
    
    public virtual Task<TEntity> GetByIdAsyncWithCache(TKey id, TimeSpan timeToLive)
    {
        return Set().FirstOrDefaultAsyncWithCache(x => x.Id.Equals(id), $"[{typeof(TEntity).Name}]-{nameof(GetByIdAsyncWithCache)}-{id}", timeToLive);
    }

    public Task<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return Set().FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Set().FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public Task<TEntity> LastOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Set().OrderByDescending(x => x.Id).FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public virtual Task<PagedListResult<TEntity>> SelectAsync(QueryOptions queryOptions, CancellationToken cancellationToken = default)
    {
        return Set().ApplyToPagedListAsync(queryOptions);
    }
}

public class BaseReadRepository<TEntity>(IDbContext dbContext) : BaseReadRepository<TEntity, Guid>(dbContext)
    where TEntity : class, IEntity<Guid>;