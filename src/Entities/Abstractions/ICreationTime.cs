using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface ICreationTime : IEntity
    {
        DateTime CreationTime { get; set; }
    }
}
