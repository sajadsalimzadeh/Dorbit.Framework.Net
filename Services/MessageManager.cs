using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class MessageManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly ConfigMessage _configs;

    public MessageManager(IServiceProvider serviceProvider, ILogger logger, IOptions<ConfigMessage> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configs = options.Value;
    }

    public Task<CommandResult> SendAsync(MessageRequest request)
    {
        if (request is MessageSmsRequest smsRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageSmsRequest, ConfigMessageSmsProvider>>();
            return Process(providers.ToList(), smsRequest, _configs.Sms);
        }

        if (request is MessageEmailRequest emailRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageEmailRequest, ConfigMessageEmailProvider>>();
            return Process(providers.ToList(), emailRequest, _configs.Email);
        }

        if (request is MessageNotificationRequest notificationRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageNotificationRequest, ConfigMessageNotificationProvider>>();
            return Process(providers.ToList(), notificationRequest, _configs.Notification);
        }

        return Task.FromResult(new CommandResult(false));
    }

    private async Task<CommandResult> Process<T, TC>(List<IMessageProvider<T, TC>> providers, T request, List<TC> configurations)
        where T : MessageRequest where TC : ConfigMessageProvider
    {
        if (App.Setting.Message is not null)
        {
            if (!string.IsNullOrEmpty(request.ProviderName))
            {
                configurations = configurations.Where(x => x.Name == request.ProviderName).ToList();
            }

            foreach (var configuration in configurations)
            {
                try
                {
                    var provider = providers.FirstOrDefault(x => x.Name == configuration.Name);
                    if (provider is null) continue;
                    provider.Configure(configuration);
                    if (!string.IsNullOrEmpty(request.TemplateType))
                    {
                        request.TemplateId = configuration.Templates[request.TemplateType];
                    }

                    var op = await provider.Send(request);
                    if (op.Success) return op;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
        }

        throw new OperationException(Errors.SendMessageFailed);
    }
}