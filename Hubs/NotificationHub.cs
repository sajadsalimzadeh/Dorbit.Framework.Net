using System;
using System.Threading.Tasks;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hubs;

public abstract class NotificationHub(HubManager hubManager) : Hub
{
    public const string GroupUserOnline = "User-Online";
    public const string OnOnlineUserUpdated = nameof(OnOnlineUserUpdated);
    
    protected readonly HubManager HubManager = hubManager;

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var userResolver = httpContext.RequestServices.GetService<IUserResolver>();
            if (userResolver.User is null)
            {
                Context.Abort();
                return;
            }
            var userId = (Guid)userResolver.User.GetId();
            HubManager.Add(userId, Context.ConnectionId);
        }

        await updateOnlineUsers();
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        HubManager.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task updateOnlineUsers()
    {
        await Clients.Group(GroupUserOnline).SendCoreAsync(OnOnlineUserUpdated, [HubManager.GetAllUserId()]);
    }
}