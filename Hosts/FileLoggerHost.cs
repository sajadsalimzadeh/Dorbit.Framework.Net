using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dorbit.Framework.Hosts;

public class FileLoggerHost(IServiceProvider serviceProvider) : BaseHostInterval(serviceProvider)
{
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);

    private static readonly ConcurrentQueue<LogRequest> Requests = new();

    public class LogRequest
    {
        public string Path { get; set; }
        public object Content { get; set; }
    }


    public static void Add(LogRequest request)
    {
        Requests.Enqueue(request);
    }

    protected override async Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        while (Requests.TryDequeue(out var request))
        {
            if(request.Content is byte[] bytes) await File.WriteAllBytesAsync(request.Path, bytes, cancellationToken);
            else if(request.Content is string text) await File.AppendAllTextAsync(request.Path, text, cancellationToken);
            else await File.AppendAllLinesAsync(request.Path, [JsonConvert.SerializeObject(request.Content)], cancellationToken);
        }
    }
}