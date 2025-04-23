using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ISoftwareAudit
{
    [MaxLength(64)]
    string SoftwareId { get; set; }
    [MaxLength(64)]
    string SoftwareName { get; set; }
}