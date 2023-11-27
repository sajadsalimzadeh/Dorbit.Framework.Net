namespace Dorbit.Framework.Entities.Abstractions;

public interface IModificationTime : IEntity
{
    DateTime? ModificationTime { get; set; }
}