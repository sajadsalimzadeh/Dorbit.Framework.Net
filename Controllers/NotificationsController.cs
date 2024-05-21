using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts;
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
    [HttpGet("{take:int}")]
    public async Task<QueryResult<List<NotificationDto>>> GetAllAsync([FromQuery] DateTime? from, [FromRoute] int? take)
    {
        var query = notificationRepository.Set().Where(x => x.ExpireTime == null || x.ExpireTime > DateTime.UtcNow);
        if (from.HasValue) query = query.Where(x => x.CreationTime > from);

        var notifications = await query.Take(take ?? 10).ToListAsync();
        var userId = GetUserId();
        notifications = notifications.Where(x => x.UserIds == null || x.UserIds.Count == 0 || x.UserIds.Contains(userId)).ToList();
        return notifications.MapTo<List<NotificationDto>>().ToQueryResult();
    }

    [HttpPost, Auth("Notification")]
    public async Task<CommandResult> NotifyAsync([FromBody] NotificationRequest request)
    {
        if (request.Test)
        {
            await notificationService.NotifyAsync(GetUserId(), request.Notification);
        }
        else
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
        }

        return Succeed();
    }
}