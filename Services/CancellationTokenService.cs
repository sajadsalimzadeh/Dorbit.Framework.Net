using System.Threading;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class CancellationTokenService : ICancellationTokenService
{
    public CancellationToken? CancellationToken { get; set; }
}