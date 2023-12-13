using Dorbit.Framework.Models.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface ISoftwareResolver
{
    ISoftwareDto GetSoftware();
}