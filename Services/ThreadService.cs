namespace Dorbit.Framework.Services;

public static class ThreadService
{
    private static Thread _mainThread = Thread.CurrentThread;

    public static Thread MainThread => _mainThread;
}