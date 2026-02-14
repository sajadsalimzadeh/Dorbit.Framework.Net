using System;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hosts;

[ServiceSingletone]
public class MessageProviderQueueHost(IServiceProvider serviceProvider) : BaseHostInterval(serviceProvider)
{
    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(10);
    protected override async Task InvokeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var messageManager = serviceProvider.GetService<MessageManager>();
        while (true)
        {
            try
            {
                var result = await messageManager.SendFromQueue();
                if(!result.Success) break;
            }
            catch(Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
        }
    }

}