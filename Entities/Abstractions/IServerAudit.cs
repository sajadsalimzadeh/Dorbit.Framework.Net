using System;
using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IServerAudit
{
    Guid? ServerId { get; set; }
    [MaxLength(64)]
    string ServerName { get; set; }
}