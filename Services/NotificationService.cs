using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Notifications;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Dorbit.Framework.Services;

public abstract class NotificationService(HubService hubService) : INotificationService
{
    private const string OnNotify = nameof(OnNotify);

    protected abstract IHubClients Clients { get; }

    public async Task NotifyAsync(NotificationDto notificationDto)
    {
        await Clients.All.SendAsync(OnNotify, notificationDto);
    }

    public async Task NotifyAsync(Guid userId, NotificationDto notificationDto)
    {
        await Clients.Clients(hubService.GetAllConnectionId(userId)).SendAsync(OnNotify, notificationDto);
    }

    public async Task NotifyAsync(List<Guid> userIds, NotificationDto notificationDto)
    {
        await Clients.Clients(hubService.GetAllConnectionId(userIds)).SendAsync(OnNotify, notificationDto);
    }

    public Task SendAsync(string method, object args)
    {
        return Clients.All.SendAsync(method, args);
    }

    public Task SendAsync(string group, string method, object args)
    {
        return Clients.Group(group).SendAsync(method, args);
    }

    public Task SendAsync(Guid userId, string method, object args)
    {
        return Clients.Clients(hubService.GetAllConnectionId(userId)).SendAsync(method, args);
    }

    public Task SendAsync(List<Guid> userIds, string method, object args)
    {
        return Clients.Clients(hubService.GetAllConnectionId(userIds)).SendAsync(method, args);
    }
}