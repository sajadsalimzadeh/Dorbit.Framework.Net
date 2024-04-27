using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletionTime
{
    DateTime? DeletionTime { get; set; }
}