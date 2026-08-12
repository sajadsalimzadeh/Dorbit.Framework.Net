using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class MessageManager(IServiceProvider serviceProvider, ILogger logger, IOptions<ConfigMessageProviders> options)
{
    private static readonly List<string> RemainCreditNotifies = new();
    private static readonly ConcurrentQueue<MessageRequest> Queue = new();

    private readonly ConfigMessageProviders _configs = options.Value;

    public Task<CommandResult> SendAsync(MessageRequest request, CancellationToken cancellationToken = default)
    {
        if (request is MessageSmsRequest smsRequest)
        {
            var providers = serviceProvider.GetServices<IMessageProvider<MessageSmsRequest, ConfigMessageSmsProvider>>();
            return ProcessAsync(providers.ToList(), smsRequest, _configs.Sms, cancellationToken);
        }

        if (request is MessageEmailRequest emailRequest)
        {
            var providers = serviceProvider.GetServices<IMessageProvider<MessageEmailRequest, ConfigMessageEmailProvider>>();
            return ProcessAsync(providers.ToList(), emailRequest, _configs.Email, cancellationToken);
        }

        if (request is MessageNotificationRequest notificationRequest)
        {
            var providers = serviceProvider.GetServices<IMessageProvider<MessageNotificationRequest, ConfigMessageNotificationProvider>>();
            return ProcessAsync(providers.ToList(), notificationRequest, _configs.Notification, cancellationToken);
        }

        return Task.FromResult(new CommandResult(false));
    }

    public Task<CommandResult> SendFromQueue(CancellationToken cancellationToken = default)
    {
        if (Queue.TryDequeue(out var item))
        {
            return SendAsync(item, cancellationToken);
        }

        return Task.FromResult(new CommandResult(false));
    }

    public void Enquee(MessageRequest request)
    {
        Queue.Enqueue(request);
    }

    private IMessageProvider<T, TC> GetProvider<T, TC>(List<IMessageProvider<T, TC>> providers, TC configuration)
        where T : MessageRequest where TC : ConfigMessageProvider
    {
        var provider = providers.FirstOrDefault(x => x.Name == configuration.ProviderName);
        if (provider is null) return null;
        provider.Configure(configuration);
        return provider;
    }

    private async Task<CommandResult> ProcessAsync<T, TConfig>(List<IMessageProvider<T, TConfig>> providers, T request, List<TConfig> configurations, CancellationToken cancellationToken = default)
        where T : MessageRequest where TConfig : ConfigMessageProvider
    {
        if (!string.IsNullOrEmpty(request.ProviderName))
        {
            configurations = configurations.Where(x => x.ProviderName == request.ProviderName).ToList();
        }

        foreach (var configuration in configurations)
        {
            try
            {
                if(configuration.FilterPrefixes.IsNotNullOrEmpty() && configuration.FilterPrefixes.All(x => !request.Receiver.StartsWith(x)))
                    continue;
                
                if (!string.IsNullOrEmpty(request.TemplateType))
                {
                    if (configuration.Templates is not null && configuration.Templates.TryGetValue(request.TemplateType, out var templateId))
                    {
                        request.TemplateId = templateId;
                    }

                    if (configuration.TemplateBodies is not null && configuration.TemplateBodies.TryGetValue(request.TemplateType, out var templateBody))
                    {
                        request.Body = templateBody;
                    }
                }

                if (request.TemplateId.IsNullOrEmpty() && request.Body.IsNullOrEmpty()) continue;

                var provider = GetProvider(providers, configuration);
                if (provider is null) continue;
                
                var op = await provider.SendAsync(request, cancellationToken);
                if (op.Success) return op;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        throw new OperationException(FrameworkErrors.SendMessageFailed);
    }

    public async Task CheckSmsProviderCredit(CancellationToken cancellationToken)
    {
        if (_configs?.Sms is null) return;

        var providers = serviceProvider.GetServices<IMessageProviderSms>().ToList();
        foreach (var configuration in _configs.Sms)
        {
            try
            {
                if (configuration.Monitoring is null || configuration.Monitoring.Numbers.Count == 0) continue;
                var provider = providers.FirstOrDefault(x => x.Name == configuration.ProviderName);
                if (provider is null) continue;
                provider.Configure(configuration);

                var credit = await provider.GetCreditMessageCountAsync(cancellationToken);
                foreach (var limit in configuration.Monitoring.Limits)
                {
                    if (credit < 0 || credit > limit) continue;
                    var key = configuration.ProviderName + limit;
                    if (RemainCreditNotifies.Contains(key)) continue;

                    foreach (var number in configuration.Monitoring.Numbers)
                    {
                        await provider.SendAsync(new MessageSmsRequest()
                        {
                            TemplateId = configuration.Monitoring.TemplateId,
                            Args = [credit.ToString()],
                            Receiver = number
                        });
                    }

                    RemainCreditNotifies.Add(key);

                    return;
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}