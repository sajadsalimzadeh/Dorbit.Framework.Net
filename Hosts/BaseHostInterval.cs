using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHostInterval(IServiceProvider serviceProvider) : BaseHost(serviceProvider, true)
{
    protected abstract TimeSpan Interval { get; }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (IsConcurrent)
        {
            new Thread(() =>
            {
                Start().Wait(stoppingToken);
            }).Start();
        }
        else
        {
            await Start();
        }

        return;

        async Task Start()
        {
            using var timer = new PeriodicTimer(Interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = ServiceProvider.CreateScope();
                    await InvokeAsync(scope.ServiceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, ex.Message);
                }
            }
        }
    }
}