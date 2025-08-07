using System.Threading;

namespace Dorbit.Framework.Extensions;

public static class CancellationTokenExtensions
{
    public static CancellationToken WithAppStoppingToken(this CancellationToken cancellationToken)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, App.StoppingToken).Token;
    }
}