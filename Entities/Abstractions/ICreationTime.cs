using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ICreationTime : IEntity
{
    DateTime CreationTime { get; set; }
}