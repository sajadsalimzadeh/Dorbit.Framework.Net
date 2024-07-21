using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Notifications;

namespace Dorbit.Framework.Services.Abstractions;

public interface INotificationService
{
    Task NotifyAsync(NotificationDto notificationDto);
    Task NotifyAsync(Guid userId, NotificationDto notificationDto);
    Task NotifyAsync(List<Guid> userIds, NotificationDto notificationDto);
    Task SendAsync(string method, object args);
    Task SendAsync(string group, string method, object args);
    Task SendAsync(Guid userId, string method, object args);
    Task SendAsync(List<Guid> userIds, string method, object args);
}