using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Services;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobCreateRequest
{
    public string Name { get; set; }
}