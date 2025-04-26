using System;
using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(CreationTime))]
public class CreateEntity<TKey> : Entity<TKey>, ICreationAudit
{
    public virtual DateTime CreationTime { get; set; } = DateTime.Now;
    public virtual string CreatorId { get; set; }

    [MaxLength(64)]
    public virtual string CreatorName { get; set; }
}

public class CreateEntity : CreateEntity<Guid>;