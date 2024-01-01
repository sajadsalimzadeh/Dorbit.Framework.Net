using System.Security.Claims;

namespace Dorbit.Framework.Services.Abstractions;

public interface IAuthService
{
    Task<bool> IsTokenValid(ClaimsPrincipal claimsPrincipal);
    Task<bool> HasAccessAsync(Guid userId, params string[] accesses);
}