using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobCreateRequest
{
    public string Name { get; set; }
    public IUserDto User { get; set; }
}