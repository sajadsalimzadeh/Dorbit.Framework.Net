using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts.Entities;

[NotMapped]
public class Audit
{
    public Guid UserId { get; set; }
    public LogAction Action { get; set; }
    public DateTime Time { get; set; }
}