using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Notifications;
using Dorbit.Framework.Entities.Abstractions;
using Innofactor.EfCoreJsonValueConverter;

namespace Dorbit.Framework.Entities;

public class Notification : CreateEntity
{
    [MaxLength(128), Required]
    public string Title { get; set; }
    [MaxLength(1024), Required]
    public string Body { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Normal;
    
    [MaxLength(128)]
    public string Icon { get; set; }
    [MaxLength(128)]
    public string Image { get; set; }

    public DateTime? ExpireTime { get; set; }

    public bool IsArchive { get; set; } = false;

    [JsonField]
    public List<Guid> UserIds { get; set; }
}