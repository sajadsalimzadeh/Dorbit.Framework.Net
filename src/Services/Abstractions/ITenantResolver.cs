using Devor.Framework.Models.Abstractions;

namespace Devor.Framework.Services.Abstractions
{
    public interface ITenantResolver
    {
        void SetTenantId(long id);
        ITenantDto GetTenant();
    }
}
