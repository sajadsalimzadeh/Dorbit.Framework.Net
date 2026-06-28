using System.Security.Claims;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Identities;

namespace Dorbit.Framework.Services.Abstractions;

public interface IIdentityService
{
    IdentityDto Identity { get; }
    ClaimsPrincipal GetClaimsPrincipal(string accessToken);
    Task<IdentityDto> ValidateAsync(IdentityRequest request);
}