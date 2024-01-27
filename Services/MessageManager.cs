using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class MessageManager
{
    private readonly AppSetting _appSetting;
    private readonly IServiceProvider _serviceProvider;

    public MessageManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _appSetting = serviceProvider.GetService<AppSetting>();
    }

    public Task<OperationResult> SendAsync(MessageRequest request)
    {
        if (request is MessageSmsRequest smsRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageSmsRequest>>().ToList();
            return Process(providers, smsRequest);
        }
        else if (request is MessageEmailRequest emailRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageEmailRequest>>().ToList();
            return Process(providers, emailRequest);
        }

        return Task.FromResult(new OperationResult(false));
    }

    private async Task<OperationResult> Process<T>(List<IMessageProvider<T>> providers, T messageRequest) where T : MessageRequest
    {
        foreach (var configuration in _appSetting.Message.Providers)
        {
            var name = configuration.GetValue<string>("Name");
            var provider = providers.FirstOrDefault(x => x.Name == name);
            if (provider is null) continue;
            provider.Configure(configuration);
            var op = await provider.Send(messageRequest);
            if (op.Success) return op;
        }

        return new OperationResult(false);
    }
}