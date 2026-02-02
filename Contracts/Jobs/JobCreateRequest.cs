using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobCreateRequest
{
    public required string Name { get; init; }
    public required Func<Job, Task> Action { get; init; }

    public IUserDto User { get; set; }
}