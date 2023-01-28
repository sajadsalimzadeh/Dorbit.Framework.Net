using Devor.Framework.Models.Abstractions;

namespace Devor.Framework.Services.Abstractions
{
    public interface IServerResolver
    {
        IServerDto GetServer();
    }
}
