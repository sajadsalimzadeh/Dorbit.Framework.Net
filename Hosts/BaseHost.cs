using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHost(IServiceProvider serviceProvider) : BackgroundService
{
    protected readonly ILogger Logger = serviceProvider.GetService<ILogger>();
    private static readonly CancellationTokenSource MainCancellationTokenSource = new();

    static BaseHost()
    {
        new Thread(() =>
        {
            while (App.MainThread.IsAlive) Thread.Sleep(1000);
            MainCancellationTokenSource.Cancel();
        }).Start();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        new Thread(Start).Start();
        return Task.CompletedTask;

        async void Start()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                await InvokeAsync(scope.ServiceProvider, MainCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, ex.Message);
            }
        }
    }

    protected abstract Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}