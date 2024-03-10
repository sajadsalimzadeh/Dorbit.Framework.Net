using System;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IHistorical : IEntity
{
    Guid HistoryId { get; set; }
    bool IsHistorical { get; set; }
}