using System;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;

namespace Dorbit.Framework.Repositories;

public class BaseRepository<TEntity, TKey> : BaseWriteRepository<TEntity, TKey>, IBaseRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    public BaseRepository(IDbContext dbContext) : base(dbContext)
    {
    }
}
public class BaseRepository<TEntity> : BaseRepository<TEntity, Guid>  where TEntity : class, IEntity<Guid>
{
    public BaseRepository(IDbContext dbContext) : base(dbContext)
    {
    }
}