using System;
using System.Security.Claims;

namespace Dorbit.Framework.Models.Abstractions;

public interface IUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ClaimsPrincipal Claims { get; set; }
}