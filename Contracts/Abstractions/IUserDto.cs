using System.Security.Claims;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface IUserDto
{
    object GetId();
    string GetUsername();
    bool IsActive();
}