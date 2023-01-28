using Devor.Framework.Attributes;
using Devor.Framework.Services.Abstractions;
using System.Threading;

namespace Devor.Framework.Services
{
    [ServiceRegisterar]
    internal class CancellationTokenService : ICancellationTokenService
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
