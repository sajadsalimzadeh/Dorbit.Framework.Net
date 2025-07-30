using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface ITenantResolver
{
    ITenantDto Tenant { get; }
}