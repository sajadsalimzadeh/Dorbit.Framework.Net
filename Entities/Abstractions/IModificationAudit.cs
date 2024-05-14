namespace Dorbit.Framework.Entities.Abstractions;

public interface IModificationAudit : IModificationTime
{
    string ModifierId { get; set; }
    string ModifierName { get; set; }
}