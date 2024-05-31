using System;

namespace Dorbit.Framework.Contracts.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public NotificationType Type { get; set; }
    public string Icon { get; set; }
    public string Image { get; set; }
    public bool IsArchive { get; set; }
    public DateTime CreationTime { get; set; }
}