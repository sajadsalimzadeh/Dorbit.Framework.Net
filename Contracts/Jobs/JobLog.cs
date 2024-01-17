using System;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobLog
{
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}