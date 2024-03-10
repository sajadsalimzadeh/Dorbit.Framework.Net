using System;
using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IEntity
{
    [Key] public Guid Id { get; set; }
}