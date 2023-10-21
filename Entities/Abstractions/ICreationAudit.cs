namespace Dorbit.Entities.Abstractions
{
    public interface ICreationAudit : ICreationTime
    {
        long? CreatorId { get; set; }
        string CreatorName { get; set; }
    }
}
