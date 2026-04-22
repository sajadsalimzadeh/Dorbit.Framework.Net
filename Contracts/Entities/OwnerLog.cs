using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts.Entities;

[NotMapped]
public class OwnerLog
{
    public Guid UserId { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime DecideTime { get; set; }
    public string Description { get; set; }
    public bool IsAccept { get; set; }
}