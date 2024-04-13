using System.Threading;

namespace Dorbit.Framework.Extensions;

public static class ThreadExtensions
{
    public static void StartAndWait(this Thread thread, int sleep = 100)
    {
        thread.Start();
        while(thread.ThreadState == ThreadState.Unstarted || thread.IsAlive) Thread.Sleep(sleep);
    }
}