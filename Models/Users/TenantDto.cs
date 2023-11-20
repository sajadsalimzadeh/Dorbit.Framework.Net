using Dorbit.Models.Abstractions;

namespace Dorbit.Models.Users;

public class TenantDto : ITenantDto
{
    public long Id { get; set; }
    public string Name { get; set; }
}