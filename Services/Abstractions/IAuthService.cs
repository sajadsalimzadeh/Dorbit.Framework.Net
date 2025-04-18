using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Services.Abstractions;

public interface IAuthService
{
    Task<IUserDto> GetUserByTokenAsync(string token);
    Task<bool> IsTokenValid(HttpContext context, ClaimsPrincipal claimsPrincipal);
    Task<IEnumerable<string>> GetAllAccessAsync();
    Task<IEnumerable<string>> GetAllAccessAsync(object userId);
    Task<bool> HasAccessAsync(string access);
    Task<bool> HasAccessAsync(object userId, string access);
}