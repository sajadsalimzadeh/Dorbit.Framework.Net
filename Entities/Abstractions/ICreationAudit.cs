namespace Dorbit.Entities.Abstractions;

public interface ICreationAudit : ICreationTime
{
    Guid? CreatorId { get; set; }
    string CreatorName { get; set; }
}