using Devor.Framework.Models.Abstractions;

namespace Devor.Framework.Services.Abstractions
{
    public interface IUserResolver
    {
        void SetUserId(long id);
        IUserDto GetUser();
    }
}
