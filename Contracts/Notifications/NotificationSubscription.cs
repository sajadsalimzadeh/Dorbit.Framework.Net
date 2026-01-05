using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts.Notifications;

[NotMapped]
public class NotificationSubscription
{
    public NotificationSubscriptionType Type { get; set; }
    public string Token { get; set; }
    public string P256DH { get; set; }
    public string Auth { get; set; }
}