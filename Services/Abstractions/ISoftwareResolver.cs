using Dorbit.Models.Abstractions;

namespace Dorbit.Services.Abstractions;

public interface ISoftwareResolver
{
    ISoftwareDto GetSoftware();
}