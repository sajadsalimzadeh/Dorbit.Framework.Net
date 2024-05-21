using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts;

public class NotificationDto
{
    public string Title { get; set; }
    public string Body { get; set; }
    public NotificationType Type { get; set; }
    public string Icon { get; set; }
    public string Image { get; set; }
}