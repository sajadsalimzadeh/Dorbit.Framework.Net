using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface IServerResolver
{
    IServerDto GetServer();
}