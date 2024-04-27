using System;
using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IEntity<TKey>
{
    [Key]
    public TKey Id { get; set; }
}

public interface IEntity : IEntity<Guid>
{
}