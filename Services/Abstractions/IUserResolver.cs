using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface IUserResolver
{
    IUserDto User { get; set; }
    Task<IUserDto> GetUserByTokenAsync(string token);
}