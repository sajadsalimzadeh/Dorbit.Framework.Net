using System.Net;
using AutoMapper;
using Dorbit.Framework;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Hosts;
using Dorbit.Framework.Installers;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;

// No Ehsaa startup, migrations, hardware, external HTTP, or service-manager operations.
using var logger = new LoggerConfiguration().CreateLogger();
await using (var provider = new ServiceCollection()
    .AddSingleton<Serilog.ILogger>(logger)
    .AddScoped<ICancellationTokenService, TokenService>()
    .AddScoped<AsyncResource>()
    .BuildServiceProvider())
{
    foreach (var concurrent in new[] { false, true })
    {
        using var worker = new OneShotWorker(provider, concurrent);
        await worker.StartAsync(default);
        await worker.Entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Check(!worker.ExecuteTask.IsCompleted, "BaseHost tracks active work");
        await worker.StopAsync(default).WaitAsync(TimeSpan.FromSeconds(3));
        Check(worker.ExecuteTask.IsCompleted && worker.Resource.Disposed,
            "BaseHost waits for asynchronous scope disposal");
    }

    using (var worker = new IntervalWorker(provider, TimeSpan.FromMilliseconds(10)))
    {
        await worker.StartAsync(default);
        await worker.Entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Check(!worker.ExecuteTask.IsCompleted, "interval worker remains tracked");
        await worker.StopAsync(default).WaitAsync(TimeSpan.FromSeconds(3));
        await Task.Delay(50);
        Check(worker.Calls == 1 && worker.Resource.Disposed && worker.ExecuteTask.IsCompleted,
            "interval worker stops without restarting work and disposes its scope");
    }
    using (var worker = new IntervalWorker(provider, TimeSpan.FromHours(1)))
    {
        await worker.StartAsync(default);
        await Task.Delay(30);
        await worker.StopAsync(default).WaitAsync(TimeSpan.FromSeconds(3));
        Check(worker.Calls == 0 && worker.ExecuteTask.IsCompleted, "stop interrupts the interval delay");
    }
}

var hostBuilder = Host.CreateApplicationBuilder();
hostBuilder.Services.AddMemoryCache();
hostBuilder.Services.AddSignalR();
hostBuilder.Services.AddSingleton<Serilog.ILogger>(logger);
hostBuilder.Services.AddSingleton<HubManager>();
hostBuilder.Services.AddSingleton<JobService>();
hostBuilder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<JobService>());
using (var host = hostBuilder.Build())
{
    await host.StartAsync();
    var jobs = host.Services.GetRequiredService<JobService>();
    var queuedReady = Signal();
    var manualReady = Signal();
    var cleaned = 0;
    Job Create(TaskCompletionSource ready) => jobs.Create(new JobCreateRequest
    {
        Name = "shutdown probe",
        Action = async job =>
        {
            ready.SetResult();
            try { await Task.Delay(Timeout.Infinite, job.CancellationToken); }
            finally { await Task.Delay(50); Interlocked.Increment(ref cleaned); }
        }
    });
    var queued = Create(queuedReady);
    var manual = Create(manualReady);
    jobs.Enqueue(queued);
    manual.Start();
    await Task.WhenAll(queuedReady.Task, manualReady.Task).WaitAsync(TimeSpan.FromSeconds(3));
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    await host.StopAsync(deadline.Token);
    Check(cleaned == 2 && queued.EndTime.HasValue && manual.EndTime.HasValue,
        "host stop cancels and waits for queued and manually started jobs");
    Check(queued.CancelTime.HasValue && manual.CancelTime.HasValue,
        "normal shutdown records job cancellation");
    try { jobs.Enqueue(queued); throw new Exception("new work accepted after stop"); }
    catch (OperationCanceledException) { }
}

// A job which ignores cancellation must respect the host's shutdown wait budget.
using (var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()))
using (var jobs = new JobService(cache, null, logger))
{
    var ready = Signal();
    var release = Signal();
    var job = jobs.Create(new JobCreateRequest
    {
        Name = "non-cooperative probe",
        Action = async _ => { ready.SetResult(); await release.Task; }
    });
    var execution = job.StartAndWaitAsync();
    await ready.Task.WaitAsync(TimeSpan.FromSeconds(3));
    using var deadline = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
    await ExpectCancellation(() => jobs.StopAsync(deadline.Token), "shutdown wait budget is respected");
    release.SetResult();
    await execution.WaitAsync(TimeSpan.FromSeconds(3));
}

foreach (var tokenSource in new[] { "argument", "property", "application" })
{
    using var cancellation = new CancellationTokenSource();
    var content = new SlowContent();
    using var http = new HttpHelper("http://fixture.invalid", new FakeHandler(content));
    if (tokenSource == "property") http.CancellationToken = cancellation.Token;
    if (tokenSource == "application") SetApplicationToken(cancellation.Token);
    try
    {
        var read = http.GetAsync<int>("slow", cancellationToken:
            tokenSource == "argument" ? cancellation.Token : default);
        await content.Entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cancellation.Cancel();
        await ExpectCancellation(() => read, "body read observes " + tokenSource + " cancellation");
        Check(content.Disposed, "failed HTTP reads dispose their response content");
    }
    finally { SetApplicationToken(default); }
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IApplication, TestApplication>();
builder.Services.AddSingleton<IMapper>(_ => new MapperConfiguration(_ => { }, NullLoggerFactory.Instance).CreateMapper());
builder.Services.AddSingleton<AsyncResource>();
var app = builder.BuildDorbit();
var owned = App.ServiceProvider.GetRequiredService<AsyncResource>();
Check(ReferenceEquals(App.ServiceProvider, app.Services), "static and host DI providers are identical");
await app.DisposeAsync();
Check(owned.Disposed, "host disposal releases resources resolved through App.ServiceProvider");
Console.WriteLine("PASS: framework shutdown regression checks");

static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
static void Check(bool value, string name)
{
    if (!value) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
}
static async Task ExpectCancellation(Func<Task> action, string name)
{
    try { await action().WaitAsync(TimeSpan.FromSeconds(3)); throw new Exception("FAIL: " + name); }
    catch (OperationCanceledException) { Console.WriteLine("PASS: " + name); }
}
static void SetApplicationToken(CancellationToken token) =>
    typeof(App).GetProperty(nameof(App.StoppingToken))!.SetValue(null, token);

sealed class TokenService : ICancellationTokenService
{
    public CancellationToken? Token { get; set; }
}
sealed class AsyncResource : IAsyncDisposable
{
    public bool Disposed;
    public async ValueTask DisposeAsync() { await Task.Delay(30); Disposed = true; }
}
sealed class OneShotWorker(IServiceProvider services, bool concurrent) : BaseHost(services, concurrent)
{
    public TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public AsyncResource Resource;
    protected override async Task InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        Resource = provider.GetRequiredService<AsyncResource>();
        Entered.SetResult();
        await Task.Delay(Timeout.Infinite, token);
    }
}
sealed class IntervalWorker(IServiceProvider services, TimeSpan interval) : BaseHostInterval(services)
{
    public TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Calls;
    public AsyncResource Resource;
    protected override TimeSpan Interval => interval;
    protected override async Task InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        Calls++;
        Resource = provider.GetRequiredService<AsyncResource>();
        Entered.TrySetResult();
        await Task.Delay(Timeout.Infinite, token);
    }
}
sealed class FakeHandler(HttpContent content) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
}
sealed class SlowContent : HttpContent
{
    public TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool Disposed;
    protected override bool TryComputeLength(out long length) { length = 0; return false; }
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) =>
        SerializeToStreamAsync(stream, context, default);
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context, CancellationToken token)
    {
        Entered.TrySetResult();
        await Task.Delay(Timeout.Infinite, token);
    }
    protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
}
sealed class TestApplication : IApplication
{
    public byte[] EncryptionKey => new byte[32];
}
