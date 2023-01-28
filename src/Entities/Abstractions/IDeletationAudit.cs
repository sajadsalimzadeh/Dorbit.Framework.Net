namespace Devor.Framework.Entities.Abstractions
{
    public interface IDeletationAudit : IDeletationTime, ISoftDelete
    {
        long? DeleterId { get; set; }
        string DeleterName { get; set; }
    }
}
