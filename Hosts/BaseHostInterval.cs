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
            new Thread(() => { Start().Wait(stoppingToken); }).Start();
        }
        else
        {
            await Start();
        }

        return;

        async Task Start()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(Interval, stoppingToken);
                    using var scope = ServiceProvider.CreateScope();
                    await InvokeAsync(scope.ServiceProvider, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, ex.Message);
                }
            }
        }
    }
}