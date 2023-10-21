using Dorbit.Attributes;
using Dorbit.Services.Abstractions;

namespace Dorbit.Services
{
    [ServiceRegisterar]
    internal class CancellationTokenService : ICancellationTokenService
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
