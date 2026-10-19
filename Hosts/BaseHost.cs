using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHost(IServiceProvider serviceProvider, bool isConcurrent = false) : BackgroundService
{
    // Retained for source compatibility. BackgroundService runs ExecuteAsync on the thread pool in .NET 10.
    protected bool IsConcurrent { get; } = isConcurrent;
    protected readonly ILogger Logger = serviceProvider.GetService<ILogger>();
    protected readonly IServiceProvider ServiceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            stoppingToken.ThrowIfCancellationRequested();
            await using var scope = ServiceProvider.CreateAsyncScope();
            var cancellationTokenService = scope.ServiceProvider.GetRequiredService<ICancellationTokenService>();
            cancellationTokenService.Token = stoppingToken;
            await InvokeAsync(scope.ServiceProvider, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, ex.Message);
        }
    }

    protected abstract Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
