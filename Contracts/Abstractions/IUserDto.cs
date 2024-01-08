namespace Dorbit.Framework.Models.Abstractions;

public interface IUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}