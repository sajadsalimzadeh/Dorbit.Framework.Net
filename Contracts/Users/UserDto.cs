using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Models.Users;

public class UserDto : IUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}