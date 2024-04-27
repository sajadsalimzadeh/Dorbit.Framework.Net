namespace Dorbit.Framework.Entities.Abstractions;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}