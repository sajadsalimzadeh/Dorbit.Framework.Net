﻿using System.Security.Claims;

namespace Dorbit.Framework.Contracts.Abstractions;

public interface IUserDto
{
    public object Id { get; set; }
    public string Username { get; set; }
    public bool IsActive { get; set; }
    public ClaimsPrincipal Claims { get; set; }
}