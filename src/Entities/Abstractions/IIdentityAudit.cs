namespace Devor.Framework.Entities.Abstractions
{
    public interface IIdentityAudit : ICreationAudit, IModificationAudit, IDeletationAudit
    {
    }
}
