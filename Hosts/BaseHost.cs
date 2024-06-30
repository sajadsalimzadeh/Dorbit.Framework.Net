using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHost : BackgroundService
{
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;
    private static readonly CancellationTokenSource MainCancellationTokenSource = new();

    static BaseHost()
    {
        new Thread(() =>
        {
            while (App.MainThread.IsAlive) Thread.Sleep(1000);
            MainCancellationTokenSource.Cancel();
        }).Start();
    }
    
    public BaseHost(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
        Logger = ServiceProvider.GetService<ILogger>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        async void Start()
        {
            try
            {
                await InvokeAsync(ServiceProvider.CreateScope().ServiceProvider, MainCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, ex.Message);
            }
        }

        new Thread(Start).Start();
        return Task.CompletedTask;
    }

    protected abstract Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}