using System;
using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ISoftwareAudit
{
    Guid? SoftwareId { get; set; }
    [MaxLength(64)]
    string SoftwareName { get; set; }
}