using System.ComponentModel.DataAnnotations;

namespace Dorbit.Entities.Abstractions
{
    public interface IEntity
    {
        [Key] public Guid Id { get; set; }
    }
}
