
using System.Collections.Generic;
using System.Security.Claims;

namespace Devor.Framework.Services.Abstractions
{
    public interface IAuthenticationService
    {
        bool HasPolicy(params string[] policies);
        IEnumerable<Claim> GetJwtClaims(string token);
    }
}
