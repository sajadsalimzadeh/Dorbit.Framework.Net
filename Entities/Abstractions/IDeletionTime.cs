using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletionTime : ISoftDelete
{
    DateTime? DeletionTime { get; set; }
}