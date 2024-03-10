using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobLogDto
{
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
}