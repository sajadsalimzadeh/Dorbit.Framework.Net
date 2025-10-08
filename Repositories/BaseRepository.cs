using System;
using Dorbit.Framework.Database.Abstractions;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;

namespace Dorbit.Framework.Repositories;

public class BaseRepository<TEntity, TKey>(IDbContext dbContext) : BaseWriteRepository<TEntity, TKey>(dbContext), IBaseRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

public class BaseRepository<TEntity>(IDbContext dbContext) : BaseRepository<TEntity, Guid>(dbContext), IBaseRepository<TEntity>
    where TEntity : class, IEntity<Guid>;