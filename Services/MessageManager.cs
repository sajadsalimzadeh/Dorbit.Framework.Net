using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class MessageManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public MessageManager(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<CommandResult> SendAsync(MessageRequest request)
    {
        if (request is MessageSmsRequest smsRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageSmsRequest>>();
            return Process(providers, smsRequest);
        }

        if (request is MessageEmailRequest emailRequest)
        {
            var providers = _serviceProvider.GetServices<IMessageProvider<MessageEmailRequest>>();
            return Process(providers, emailRequest);
        }

        return Task.FromResult(new CommandResult(false));
    }

    private async Task<CommandResult> Process<T>(IEnumerable<IMessageProvider<T>> providers, T messageRequest) where T : MessageRequest
    {
        if (App.Setting.Message is not null)
        {
            foreach (var configuration in App.Setting.Message.Providers)
            {
                try
                {
                    var provider = providers.FirstOrDefault(x => x.Name == configuration.Name);
                    if (provider is null) continue;
                    provider.Configure(configuration);
                    if (!string.IsNullOrEmpty(messageRequest.TemplateType))
                    {
                        messageRequest.TemplateId = configuration.Templates[messageRequest.TemplateType];
                    }

                    var op = await provider.Send(messageRequest);
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