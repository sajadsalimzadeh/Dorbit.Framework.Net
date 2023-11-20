using Dorbit.Models.Abstractions;

namespace Dorbit.Services.Abstractions;

public interface IAuthService
{
    Task<IUserDto> GetUserByTokenAsync(string key);
    Task<bool> HasAccessAsync(Guid userId, params string[] accesses);
}