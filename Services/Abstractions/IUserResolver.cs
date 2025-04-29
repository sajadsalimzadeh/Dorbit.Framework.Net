using System.Security.Claims;
using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Services.Abstractions;

public interface IUserResolver
{
    IUserDto User { get; set; }
}