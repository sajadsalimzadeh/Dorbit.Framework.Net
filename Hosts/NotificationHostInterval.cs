using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceSingletone]
public class NotificationHostInterval(IServiceProvider serviceProvider) : BaseHostInterval(serviceProvider)
{
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(10);
    protected override async Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var notificationService = serviceProvider.GetRequiredService<NotificationService>();
        while (await notificationService.SendAsync(cancellationToken)) await Task.Delay(1000, cancellationToken);
    }

}