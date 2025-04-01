using System;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobLogger(Job job) : ILogger, IDisposable
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        job.Logs.Add(new JobLog()
        {
            Level = logLevel,
            Exception = exception,
            Message = formatter(state, exception),
        });
    }

    public void Dispose()
    {
    }
}