using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dorbit.Framework.Utils;

public static class TaskUtil
{
    public static async Task Delay(int ms, CancellationToken cancellationToken)
    {
        try 
        {
            await Task.Delay(ms, cancellationToken);
        } catch (OperationCanceledException)
        {
            return;
        }
    }
    
    public static async Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        try 
        {
            await Task.Delay(timeSpan, cancellationToken);
        } catch (OperationCanceledException)
        {
            return;
        }
    }
}