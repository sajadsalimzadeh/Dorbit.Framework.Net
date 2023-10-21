using Dorbit.Models.Abstractions;

namespace Dorbit.Services.Abstractions
{
    public interface IUserResolver
    {
        void SetUserId(long id);
        IUserDto GetUser();
    }
}
