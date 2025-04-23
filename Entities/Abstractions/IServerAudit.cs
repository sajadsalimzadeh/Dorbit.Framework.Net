using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IServerAudit
{
    [MaxLength(64)]
    string ServerId { get; set; }
    [MaxLength(64)]
    string ServerName { get; set; }
}