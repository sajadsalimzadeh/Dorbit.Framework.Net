using System;
using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ITenantAudit : IEntity
{
    Guid? TenantId { get; set; }
    [MaxLength(64)]
    string TenantName { get; set; }
}