using Dorbit.Models.Abstractions;

namespace Dorbit.Services.Abstractions
{
    public interface IServerResolver
    {
        IServerDto GetServer();
    }
}
