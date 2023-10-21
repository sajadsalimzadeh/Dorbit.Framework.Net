using Dorbit.Models.Abstractions;

namespace Dorbit.Models.Users
{
    public class UserDto : IUserDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
