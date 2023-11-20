using Dorbit.Entities.Abstractions;

namespace Dorbit.Entities;

public class CreateEntity : Entity, ICreationAudit
{
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public string CreatorName { get; set; }
}