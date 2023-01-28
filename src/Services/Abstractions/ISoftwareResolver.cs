using Devor.Framework.Models.Abstractions;

namespace Devor.Framework.Services.Abstractions
{
    public interface ISoftwareResolver
    {
        ISoftwareDto GetSoftware();
    }
}
