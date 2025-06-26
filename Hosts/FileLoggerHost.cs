using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Services;

namespace Dorbit.Framework.Hosts;

public class FileLoggerHost(IServiceProvider serviceProvider, LoggerService loggerService) : BaseHostInterval(serviceProvider)
{
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);
    
    protected override async Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        while (loggerService.FileLogs.TryDequeue(out var request))
        {
            if(request.Content is byte[] bytes) await File.WriteAllBytesAsync(request.Path, bytes, cancellationToken);
            else if(request.Content is string text) await File.AppendAllTextAsync(request.Path, text, cancellationToken);
            else await File.AppendAllLinesAsync(request.Path, [JsonSerializer.Serialize(request.Content)], cancellationToken);
        }
    }
}