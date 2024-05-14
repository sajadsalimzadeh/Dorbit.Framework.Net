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
    public ConcurrentDictionary<Guid, string> Connections { get; } = new();

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var userResolver = httpContext.RequestServices.GetService<IUserResolver>();
            Connections[(Guid)userResolver.User.Id] = Context.ConnectionId;
        }
        
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var item = Connections.FirstOrDefault(x => x.Value == Context.ConnectionId);
        Connections.TryRemove(item.Key, out _);
        return base.OnDisconnectedAsync(exception);
    }
}