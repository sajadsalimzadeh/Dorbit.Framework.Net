using Devor.Framework.Entities.Abstractions;
using Devor.Framework.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Devor.Framework.Entities
{
    [Index(nameof(EntityType)), Index(nameof(EntityType), nameof(ReferenceId))]
    public class EntityLog : Entity, ICreationAudit
    {
        [StringLength(64), Required]
        public string Module { get; set; }
        [StringLength(64), Required]
        public string EntityType { get; set; }
        public long ReferenceId { get; set; }
        public string Data { get; set; }
        public LogAction Action { get; set; }
        public long? CreatorId { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
