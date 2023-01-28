using Devor.Framework.Models.Abstractions;

namespace Devor.Framework.Models.Users
{
    public class UserDto : IUserDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
