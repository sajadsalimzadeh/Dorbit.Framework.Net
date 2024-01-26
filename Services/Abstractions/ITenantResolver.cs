using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface ITenantResolver
{
    void SetTenantId(long id);
    ITenantDto GetTenant();
}