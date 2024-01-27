using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Contracts.Users;

public class BaseTenantDto : ITenantDto
{
    public long Id { get; set; }
    public string Name { get; set; }
}