using Dorbit.Framework.Attributes;
using Dorbit.Framework.Models.Abstractions;
using Dorbit.Framework.Models.Users;
using Dorbit.Framework.Services.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class UserResolver : IUserResolver
{
    public IUserDto User { get; set; }
}