using Dorbit.Models.Abstractions;

namespace Dorbit.Services.Abstractions;

public interface IUserResolver
{
    IUserDto User { get; set; }
}