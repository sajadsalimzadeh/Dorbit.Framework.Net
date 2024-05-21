using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts;

public class NotificationRequest
{
    public List<Guid> UserIds { get; set; }
    public NotificationDto Notification { get; set; }
}