using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHost : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    protected readonly ILoggerService LoggerService;

    protected abstract int IntervalInSec { get; }

    public BaseHost(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        LoggerService = serviceProvider.GetService<ILoggerService>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            for (var i = 0; !stoppingToken.IsCancellationRequested; i ++)
            {
                try
                {
                    if (i % IntervalInSec != 0) continue;
                    try
                    {
                        await InvokeAsync(_serviceProvider.CreateScope().ServiceProvider, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        LoggerService?.LogError(ex.Message);
                    }
                }
                finally
                {
                    await Task.Delay(100);
                }
            }
        });
    }

    protected abstract Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}