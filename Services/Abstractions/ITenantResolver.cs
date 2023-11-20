using Dorbit.Models.Abstractions;

namespace Dorbit.Services.Abstractions;

public interface ITenantResolver
{
    void SetTenantId(long id);
    ITenantDto GetTenant();
}