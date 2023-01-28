namespace Devor.Framework.Entities.Abstractions
{
    public interface ISoftDelete : IEntity
    {
        bool IsDeleted { get; set; }
    }
}
