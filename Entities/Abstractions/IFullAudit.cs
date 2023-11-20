namespace Dorbit.Entities.Abstractions;

public interface IFullAudit : ICreationAudit, IModificationAudit, IDeletationAudit
{
}