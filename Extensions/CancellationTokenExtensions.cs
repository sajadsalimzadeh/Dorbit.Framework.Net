using System;
using System.Threading;

namespace Dorbit.Framework.Extensions;

public static class CancellationTokenExtensions
{
    public static CancellationToken WithAppStoppingToken(this CancellationToken cancellationToken)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, App.StoppingToken).Token;
    }
    public static CancellationToken WithTimeout(this CancellationToken cancellationToken, TimeSpan timeSpan)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(timeSpan);
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;
    }
}