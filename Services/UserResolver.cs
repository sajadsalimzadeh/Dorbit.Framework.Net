using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Services.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class UserResolver : IUserResolver
{
    public IUserDto User { get; set; }
}