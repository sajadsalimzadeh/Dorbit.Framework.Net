using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ITenantAudit : IEntity
{
    [MaxLength(64)]
    string TenantId { get; set; }
    [MaxLength(64)]
    string TenantName { get; set; }
}