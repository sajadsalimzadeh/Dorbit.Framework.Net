using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface IDeletationTime : IEntity
    {
        DateTime? DeletationTime { get; set; }
    }
}
