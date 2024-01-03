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
}