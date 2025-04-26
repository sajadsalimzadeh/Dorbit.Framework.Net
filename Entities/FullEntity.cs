using System;
using Dorbit.Framework.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(CreationTime))]
public class FullEntity<TKey> : Entity<TKey>, IFullAudit
{
    public virtual DateTime CreationTime { get; set; }
    public virtual string CreatorId { get; set; }
    public string CreatorName { get; set; }
    public virtual DateTime ModificationTime { get; set; }
    public virtual string ModifierId { get; set; }
    public virtual string ModifierName { get; set; }
    public virtual DateTime? DeletionTime { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual string DeleterId { get; set; }
    public virtual string DeleterName { get; set; }
}

public class FullEntity : FullEntity<Guid>, IEntity;