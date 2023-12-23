using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface IAuthService
{
    Task<IUserDto> GetUserByTokenAsync(string token);
    Task<bool> HasAccessAsync(Guid userId, params string[] accesses);
}