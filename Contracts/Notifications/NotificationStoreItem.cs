namespace Dorbit.Framework.Contracts.Notifications;

public class NotificationStoreItem
{
    public NotificationSubscription Subscription { get; set; }
    public NotificationRequest Request { get; set; }
}