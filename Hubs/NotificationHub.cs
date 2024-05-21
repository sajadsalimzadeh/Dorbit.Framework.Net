using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Hubs.Abstractions;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hubs;

public abstract class NotificationHub : Hub
{
    protected HubService HubService;

    protected NotificationHub(HubService hubService)
    {
        HubService = hubService;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var userResolver = httpContext.RequestServices.GetService<IUserResolver>();
            var userId = (Guid)userResolver.User.Id;
            HubService.Add(userId, Context.ConnectionId);
        }
        
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        HubService.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}