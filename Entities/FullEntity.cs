using Dorbit.Entities.Abstractions;

namespace Dorbit.Entities;

public class FullEntity : CreateEntity, IFullAudit
{
    public DateTime? ModificationTime { get; set; }
    public Guid? ModifierId { get; set; }
    public string ModifierName { get; set; }
    public DateTime? DeletionTime { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeleterId { get; set; }
    public string DeleterName { get; set; }
}