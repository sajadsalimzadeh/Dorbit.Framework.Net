using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Services.Abstractions;

public interface IAuthService
{
    Task<bool> IsTokenValid(HttpContext context, ClaimsPrincipal claimsPrincipal);
    Task<bool> HasAccessAsync(Guid userId, params string[] accesses);
}