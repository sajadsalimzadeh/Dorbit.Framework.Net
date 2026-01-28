using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts.Entities;

[NotMapped]
public class StatusLog<T> where T : Enum
{
    public T Status { get; set; }
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }
    public string Description { get; set; }

    public StatusLog()
    {
    }

    public StatusLog(T status, Guid? userId, string description = null)
    {
        Status = status;
        UserId = userId;
        Description = description;
    }
}