namespace Dorbit.Entities.Abstractions;

public interface ITenantAudit : IEntity
{
    long? TenantId { get; set; }
    string TenantName { get; set; }
}