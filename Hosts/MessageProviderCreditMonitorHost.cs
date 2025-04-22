﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class MessageProviderCreditMonitorHost(IServiceProvider serviceProvider) : BaseHostInterval(serviceProvider)
{
    protected override Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var messageManager = serviceProvider.GetService<MessageManager>();
        return messageManager.CheckSmsProviderCredit();
    }

    protected override TimeSpan Interval { get; } = TimeSpan.FromMinutes(15);
}