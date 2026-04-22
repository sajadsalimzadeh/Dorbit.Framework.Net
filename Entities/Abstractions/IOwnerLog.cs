using System;
using System.Collections.Generic;
using Dorbit.Framework.Contracts.Entities;
using Dorbit.Framework.Utils.Json;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IOwnerLog
{
    public Guid? OwnerId { get; set; }
    public Guid? OwnerRequestId { get; set; }
    public DateTime? OwnerRequestTime { get; set; }

    [JsonField]
    public List<OwnerLog> OwnerLogs { get; set; }
}