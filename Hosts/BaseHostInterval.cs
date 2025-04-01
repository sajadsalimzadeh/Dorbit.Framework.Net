using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHostInterval(IServiceProvider serviceProvider) : BaseHost(serviceProvider)
{
    protected abstract TimeSpan Interval { get; }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        new Thread(Start).Start();
        return Task.CompletedTask;

        async void Start()
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