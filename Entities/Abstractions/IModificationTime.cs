namespace Dorbit.Entities.Abstractions;

public interface IModificationTime : IEntity
{
    DateTime? ModificationTime { get; set; }
}