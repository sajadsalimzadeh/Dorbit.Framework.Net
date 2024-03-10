using System;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Entities;

public class FullEntity : CreateEntity, IFullAudit
{
    public DateTime? ModificationTime { get; set; }
    public Guid? ModifierId { get; set; }
    public string ModifierName { get; set; }
    public DateTime? DeletionTime { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeleterId { get; set; }
    public string DeleterName { get; set; }
}