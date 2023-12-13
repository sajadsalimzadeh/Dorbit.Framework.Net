using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Entities;

public abstract class Entity : IEntity
{
    public virtual Guid Id { get; set; }
}