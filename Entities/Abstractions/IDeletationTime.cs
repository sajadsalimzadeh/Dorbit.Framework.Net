namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletationTime : IEntity
{
    DateTime? DeletionTime { get; set; }
}