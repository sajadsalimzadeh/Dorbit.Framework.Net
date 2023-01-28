using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface IHistorical : IEntity
    {
        Guid HistoryId { get; set; }
        bool IsHistorical { get; set; }
    }
}
