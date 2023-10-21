using System.Security.Claims;

namespace Dorbit.Services.Abstractions
{
    public interface IAuthenticationService
    {
        bool HasPolicy(params string[] policies);
        IEnumerable<Claim> GetJwtClaims(string token);
    }
}
