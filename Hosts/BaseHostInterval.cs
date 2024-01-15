using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHostInterval : BaseHost
{
    protected BaseHostInterval(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected abstract TimeSpan Interval { get; }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async void Start()
        {
            using var timer = new PeriodicTimer(Interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await InvokeAsync(ServiceProvider.CreateScope().ServiceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, ex.Message);
                }
            }
        }

        new Thread(Start).Start();
        return Task.CompletedTask;
    }
}