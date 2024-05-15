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

public sealed class HubStore
{
    private ConcurrentDictionary<Guid, List<string>> UserToConnections { get; } = new();
    private ConcurrentDictionary<string, Guid> ConnectionToUsers { get; } = new();

    public List<string> GetAllConnectionId(Guid userId)
    {
        return UserToConnections.TryGetValue(userId, out var value) ? value : [];
    }

    public List<string> GetAllConnectionId(List<Guid> userIds)
    {
        var connectionIds = new List<string>();
        foreach (var userId in userIds)
        {
            if(UserToConnections.TryGetValue(userId, out var value)) connectionIds.AddRange(value);
        }

        return connectionIds;
    }

    public Guid? GetUserId(string connectionId)
    {
        return ConnectionToUsers.GetValueOrDefault(connectionId);
    }

    public void Add(Guid userId, string connectionId)
    {
        var connections = UserToConnections.GetOrAdd(userId, []);
        ConnectionToUsers[connectionId] = userId;
        connections.Add(connectionId);
    }
    
    public void Remove(string connectionId)
    {
        if (ConnectionToUsers.TryGetValue(connectionId, out var userId))
        {
            ConnectionToUsers.TryRemove(connectionId, out _);
            UserToConnections.TryRemove(userId, out _);
        }
    }
}

public abstract class NotificationHub : Hub
{
    public HubStore Store { get; } = new();
    
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var userResolver = httpContext.RequestServices.GetService<IUserResolver>();
            var userId = (Guid)userResolver.User.Id;
            Store.Add(userId, Context.ConnectionId);
        }
        
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Store.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}