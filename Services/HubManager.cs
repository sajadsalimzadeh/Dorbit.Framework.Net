using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class HubManager(IHubContext<BaseHub> hubContext)
{
    public IHubContext<BaseHub> HubContext => hubContext;
    private ConcurrentDictionary<Guid, List<string>> UserToConnections { get; } = new();
    private ConcurrentDictionary<string, Guid> ConnectionToUsers { get; } = new();

    public List<Guid> GetAllUserId()
    {
        return UserToConnections.Where(x => x.Value.Count > 0).Select(x => x.Key).ToList();
    }

    public List<string> GetAllConnectionId(Guid userId)
    {
        return UserToConnections.TryGetValue(userId, out var value) ? value : [];
    }

    public List<string> GetAllConnectionId(List<Guid> userIds)
    {
        var connectionIds = new List<string>();
        foreach (var userId in userIds)
        {
            if (UserToConnections.TryGetValue(userId, out var value)) connectionIds.AddRange(value);
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

    public int Count()
    {
        return ConnectionToUsers.Count;
    }

    public void Remove(string connectionId)
    {
        if (!ConnectionToUsers.TryGetValue(connectionId, out var userId)) return;
        ConnectionToUsers.TryRemove(connectionId, out _);
        if (!UserToConnections.TryGetValue(userId, out var connections)) return;
        connections.Remove(connectionId);
        if (connections.Count == 0)
        {
            UserToConnections.Remove(userId, out _);
        }
    }
}