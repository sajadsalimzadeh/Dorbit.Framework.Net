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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Controllers;

[Auth]
public class NotificationsController(NotificationRepository notificationRepository) : BaseApiController
{
    [HttpGet("{take:int}")]
    public async Task<QueryResult<List<NotificationDto>>> GetAllAsync([FromQuery] DateTime? from, [FromRoute] int take = 10)
    {
        var query = notificationRepository.Set().Where(x => x.ExpireTime == null || x.ExpireTime > DateTime.UtcNow);
        if (from.HasValue) query = query.Where(x => x.CreationTime > from);

        var notifications = await query.OrderByDescending(x => x.CreationTime).Take(take).ToListAsync();
        var userId = GetUserId();
        notifications = notifications.Where(x => x.UserIds == null || x.UserIds.Contains(userId)).ToList();
        return notifications.MapTo<List<NotificationDto>>().ToQueryResult();
    }
}