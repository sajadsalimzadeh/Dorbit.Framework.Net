using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHost : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected readonly ILogger Logger;

    protected abstract TimeSpan Interval { get; }

    public BaseHost(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILogger>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(Interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await InvokeAsync(_serviceProvider.CreateScope().ServiceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, ex.Message);
                }
            }
        }, stoppingToken);
    }

    protected abstract Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}