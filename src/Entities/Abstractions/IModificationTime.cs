using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface IModificationTime : IEntity
    {
        DateTime? ModificationTime { get; set; }
    }
}
