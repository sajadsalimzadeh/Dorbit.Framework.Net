namespace Dorbit.Framework.Entities.Abstractions;

public interface IModificationAudit : IModificationTime
{
    Guid? ModifierId { get; set; }
    string ModifierName { get; set; }
}