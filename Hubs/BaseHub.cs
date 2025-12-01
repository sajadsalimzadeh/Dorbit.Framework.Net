using System;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Identities;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Hubs;

public class BaseHub(HubManager hubService) : Hub
{
    public const string GroupUserOnline = "User-Online";
    public const string GroupJobStatus = "Job-Status";
    public const string OnOnlineUserUpdated = nameof(OnOnlineUserUpdated);
    
    protected readonly HubManager HubManager = hubService;

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null)
        {
            var identityService = httpContext.RequestServices.GetService<IIdentityService>();
            var identity = await identityService.ValidateAsync(httpContext.GetIdentityRequest());
            if (identity is null)
            {
                Context.Abort();
                return;
            }
            var userId = (Guid)identity.User.GetId();
            HubManager.Add(userId, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupJobStatus);
        }

        await UpdateOnlineUsers();
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        HubManager.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task UpdateOnlineUsers()
    {
        await Clients.Group(GroupUserOnline).SendCoreAsync(OnOnlineUserUpdated, [HubManager.GetAllUserId()]);
    }
}