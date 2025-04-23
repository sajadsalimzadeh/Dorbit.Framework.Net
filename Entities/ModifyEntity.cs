using System;
using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Entities.Abstractions;

namespace Dorbit.Framework.Entities;

public class ModifyEntity<TKey> : CreateEntity<TKey>, IModificationAudit
{
    public DateTime ModificationTime { get; set; }
    public string ModifierId { get; set; }
    
    [MaxLength(64)]
    public string ModifierName { get; set; }
}

public class ModifyEntity : ModifyEntity<Guid>
{
    
}