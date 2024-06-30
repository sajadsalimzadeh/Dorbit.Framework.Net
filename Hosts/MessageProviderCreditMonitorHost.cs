using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class MessageProviderCreditMonitorHost : BaseHostInterval
{
    public MessageProviderCreditMonitorHost(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var messageManager = serviceProvider.GetService<MessageManager>();
        return messageManager.CheckSmsProviderCredit();
    }

    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);
}