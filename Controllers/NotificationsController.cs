using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Notifications;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Controllers;

[Auth]
public class NotificationsController(NotificationRepository notificationRepository, INotificationService notificationService) : BaseApiController
{
    [HttpGet]
    public async Task<QueryResult<List<NotificationDto>>> GetAllAsync()
    {
        var notifications = await notificationRepository.Set()
            .Where(x => x.IsArchive && (x.ExpireTime == null || x.ExpireTime > DateTime.UtcNow))
            .OrderByDescending(x => x.CreationTime).Take(50).ToListAsync();
        var userId = GetUserId();
        notifications = notifications.Where(x => x.UserIds == null || x.UserIds.Count == 0 || x.UserIds.Contains(userId)).ToList();
        return notifications.MapTo<List<NotificationDto>>().ToQueryResult();
    }

    [HttpGet("Last")]
    public async Task<QueryResult<NotificationDto>> GetLastAsync()
    {
        var notifications = await notificationRepository.Set()
            .Where(x => x.ExpireTime == null || x.ExpireTime > DateTime.UtcNow)
            .OrderByDescending(x => x.CreationTime).Take(50).ToListAsync();
        var userId = GetUserId();
        var notification = notifications.FirstOrDefault(x => x.UserIds == null || x.UserIds.Count == 0 || x.UserIds.Contains(userId));
        return notification.MapTo<NotificationDto>().ToQueryResult();
    }

    [HttpPost, Auth("Notification")]
    public async Task<CommandResult> NotifyAsync([FromBody] NotificationRequest request)
    {
        var entity = request.Notification.MapTo<Notification>();
        entity.UserIds = request.UserIds;
        var model = await notificationRepository.InsertAsync(entity).MapToAsync<Notification, NotificationDto>();

        if (request.UserIds is not null && request.UserIds.Count > 0)
        {
            await notificationService.NotifyAsync(request.UserIds, model);
        }
        else
        {
            await notificationService.NotifyAsync(model);
        }

        return Succeed();
    }

    [HttpPost("Empty"), Auth("Notification")]
    public Task<CommandResult> NotifyAsync()
    {
        return NotifyAsync(new NotificationRequest()
        {
            Notification = new NotificationDto()
            {
                Title = "",
                Body = ""
            }
        });
    }

    [HttpDelete("{id:guid}")]
    public Task<QueryResult<NotificationDto>> DeleteAsync([FromRoute] Guid id)
    {
        return notificationRepository.DeleteAsync(id).MapToAsync<Notification, NotificationDto>().ToQueryResultAsync();
    }
}