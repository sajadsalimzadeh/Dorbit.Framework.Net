namespace Dorbit.Entities.Abstractions;

public interface IServerAudit
{
    long? ServerId { get; set; }
    string ServerName { get; set; }
}