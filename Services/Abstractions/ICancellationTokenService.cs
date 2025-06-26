using System.Threading;

namespace Dorbit.Framework.Services.Abstractions;

public interface ICancellationTokenService
{
    CancellationToken? RequestCancellationToken { get; set; }
}