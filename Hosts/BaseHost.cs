using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHost(IServiceProvider serviceProvider, bool isConcurrent = false) : BackgroundService
{
    protected bool IsConcurrent { get; } = isConcurrent;
    protected readonly ILogger Logger = serviceProvider.GetService<ILogger>();
    protected readonly IServiceProvider ServiceProvider = serviceProvider;

    static BaseHost()
    {
        // new Thread(() =>
        // {
        //     var mainCancellationTokenSource = new CancellationTokenSource();
        //     App.MainCancellationToken = mainCancellationTokenSource.Token;
        //     while (App.MainThread.IsAlive) Thread.Sleep(1000);
        //     mainCancellationTokenSource.Cancel();
        // }).Start();
    }

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

    protected abstract Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}