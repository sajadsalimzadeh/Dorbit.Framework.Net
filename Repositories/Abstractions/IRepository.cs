using System;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Repositories.Abstractions;

public interface IBaseRepository<TEntity, TKey> : IWriterRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
}

public interface IBaseRepository<TEntity> : IBaseRepository<TEntity, Guid> where TEntity : class, IEntity<Guid>;