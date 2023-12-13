namespace Dorbit.Framework.Entities.Abstractions;

public interface ISoftwareAudit
{
    long? SoftwareId { get; set; }
    string SoftwareName { get; set; }
}