namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletationAudit : IDeletationTime, ISoftDelete
{
    Guid? DeleterId { get; set; }
    string DeleterName { get; set; }
}