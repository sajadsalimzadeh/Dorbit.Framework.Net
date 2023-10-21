namespace Dorbit.Entities.Abstractions
{
    public interface ISoftDelete : IEntity
    {
        bool IsDeleted { get; set; }
    }
}
