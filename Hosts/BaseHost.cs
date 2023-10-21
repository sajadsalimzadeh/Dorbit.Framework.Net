using Dorbit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dorbit.Hosts;

public abstract class BaseHost : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    protected Thread Thread { get; set; }

    /// <summary>
    /// Seconds
    /// </summary>
    protected abstract int Interval { get; }

    public BaseHost(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Start()
    {
        Thread = new Thread(() =>
        {
            for (long i = 0; ThreadService.MainThread.IsAlive; i++)
            {
                try
                {
                    if (i % Interval != 0) continue;
                    var sp = _serviceProvider.CreateScope().ServiceProvider;
                    var logger = sp.GetService<ILogger>();
                    try
                    {
                        Invoke(sp);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex.Message);
                    }
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        });
        Thread.Start();
    }

    protected abstract void Invoke(IServiceProvider serviceProvider);
}