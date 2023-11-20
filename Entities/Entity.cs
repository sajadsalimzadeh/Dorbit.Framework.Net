using Dorbit.Entities.Abstractions;

namespace Dorbit.Entities;

public abstract class Entity : IEntity
{
    public virtual Guid Id { get; set; }
}