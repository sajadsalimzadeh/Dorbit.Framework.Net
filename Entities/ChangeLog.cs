using System.ComponentModel.DataAnnotations;
using Dorbit.Framework.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Dorbit.Framework.Entities;

[Index(nameof(EntityType)), Index(nameof(EntityType), nameof(ReferenceId))]
public class ChangeLog : CreateEntity
{
    [MaxLength(64), Required]
    public string Module { get; set; }

    [MaxLength(64), Required]
    public string EntityType { get; set; }

    [MaxLength(64)]
    public string ReferenceId { get; set; }
    
    public string Data { get; set; }
    public LogAction Action { get; set; }
}