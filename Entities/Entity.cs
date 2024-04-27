using System;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Entities;

public abstract class Entity<TKey> : IEntity<TKey>
{
    public virtual TKey Id { get; set; }
}

public abstract class Entity : Entity<Guid>;