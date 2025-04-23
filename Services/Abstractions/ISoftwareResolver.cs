using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface ISoftwareResolver
{
    ISoftwareDto Software { get; set; }
}