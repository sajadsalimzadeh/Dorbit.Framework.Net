using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Models.Users;

public class TenantDto : ITenantDto
{
    public long Id { get; set; }
    public string Name { get; set; }
}