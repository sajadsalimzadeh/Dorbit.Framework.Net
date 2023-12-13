using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface IServerResolver
{
    IServerDto GetServer();
}