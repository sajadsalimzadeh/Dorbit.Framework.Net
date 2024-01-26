using System;
using System.Security.Claims;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface IUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public ClaimsPrincipal Claims { get; set; }
}