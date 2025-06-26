using System.Collections.Concurrent;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Loggers;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;


[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class LoggerService
{
    internal readonly ConcurrentQueue<LogEntityRequest> DatabaseLogs = new();
    internal readonly ConcurrentQueue<LogFileRequest> FileLogs = new();
    
    public void Log(LogEntityRequest entityRequest)
    {
        DatabaseLogs.Enqueue(entityRequest);
    }
    
    public void LogToFile(LogFileRequest entityRequest)
    {
        FileLogs.Enqueue(entityRequest);
    }
}