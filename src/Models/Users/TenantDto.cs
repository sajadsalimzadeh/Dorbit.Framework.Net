using Devor.Framework.Models.Abstractions;

namespace Devor.Framework.Models.Users
{
    public class TenantDto : ITenantDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
