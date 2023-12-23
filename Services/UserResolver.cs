using Dorbit.Framework.Attributes;
using Dorbit.Framework.Models.Abstractions;
using Dorbit.Framework.Models.Users;
using Dorbit.Framework.Services.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class UserResolver : IUserResolver
{
    private readonly JwtService _jwtService;
    
    public IUserDto User { get; set; }

    public UserResolver(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public async Task<IUserDto> GetUserByTokenAsync(string token)
    {
        if (await _jwtService.TryValidateTokenAsync(token, out var securityToken, out var claims))
        {
            var id = claims.FindFirst("Id")?.Value;
            return new UserDto()
            {
                Id = Guid.Parse(id ?? ""),
                Name = claims.FindFirst("Name")?.Value
            };
        }

        return default;
    }
}