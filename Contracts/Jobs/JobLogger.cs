using System;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobLogger : ILogger, IDisposable
{
    private readonly Job _job;

    public JobLogger(Job job)
    {
        _job = job;
    }

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
        _job.Logs.Add(new JobLog()
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