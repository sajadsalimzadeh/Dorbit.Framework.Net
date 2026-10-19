using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

public abstract class BaseHostInterval(IServiceProvider serviceProvider) : BaseHost(serviceProvider, true)
{
    protected abstract TimeSpan Interval { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                stoppingToken.ThrowIfCancellationRequested();
                await using var scope = ServiceProvider.CreateAsyncScope();
                var cancellationTokenService = scope.ServiceProvider.GetRequiredService<ICancellationTokenService>();
                cancellationTokenService.Token = stoppingToken;
                await InvokeAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, ex.Message);
            }
        }
    }
}
