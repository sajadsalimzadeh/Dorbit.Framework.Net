namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletionAudit : IDeletionTime
{
    string DeleterId { get; set; }
    string DeleterName { get; set; }
}