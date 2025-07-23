using System.Collections.Generic;
using Dorbit.Framework.Contracts.Entities;
using Dorbit.Framework.Utils.Json;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IAuditHistory
{
    [JsonField]
    public List<Audit> Audits { get; set; }
}