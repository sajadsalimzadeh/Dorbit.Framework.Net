using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Notifications;
using Microsoft.Extensions.Options;
using Serilog;
using WebPush;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class NotificationService(IOptions<ConfigWebPush> configWebPushOptions, ILogger logger)
{
    private readonly ConcurrentQueue<NotificationStoreItem> _storeItems = new();

    public void Enqueue(NotificationStoreItem item)
    {
        _storeItems.Enqueue(item);
    }

    public async Task<bool> SendAsync(CancellationToken cancellationToken = default)
    {
        if (_storeItems.TryDequeue(out var item))
        {
            try
            {
                if (item.Subscription.Type == NotificationSubscriptionType.WebPush)
                {
                    var webPushClient = new WebPushClient();

                    webPushClient.SetVapidDetails(
                        subject: configWebPushOptions.Value.MailTo,
                        publicKey: configWebPushOptions.Value.PublicKey,
                        privateKey: configWebPushOptions.Value.PrivateKey
                    );

                    var payload = JsonSerializer.Serialize(new
                    {
                        Notification = new
                        {
                            Title = item.Request.Title,
                            Body = item.Request.Body,
                            Icon = item.Request.Icon,
                            Data = new
                            {
                                url = item.Request.Url
                            }
                        }
                    }, JsonSerializerOptions.Web);

                    await webPushClient.SendNotificationAsync(new PushSubscription(item.Subscription.Token, item.Subscription.P256DH, item.Subscription.Auth), payload, cancellationToken: cancellationToken);
                }
                else if(item.Subscription.Type == NotificationSubscriptionType.Expo)
                {
                    var httpClient = new HttpClient() { BaseAddress = new Uri("https://exp.host/--/api/v2/push/send") };
                    var responseMessage = await httpClient.PostAsJsonAsync("", new NotificationExpoDto()
                    {
                        Token = item.Subscription.Token,
                        Title = item.Request.Title,
                        Body = item.Request.Body,
                        Data = new Dictionary<string, string>
                        {
                            { "url", item.Request.Url }
                        }
                    }, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            
            return true;
        }

        return false;
    }
}