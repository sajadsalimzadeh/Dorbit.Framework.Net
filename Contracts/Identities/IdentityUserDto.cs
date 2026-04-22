using System;
using Dorbit.Framework.Contracts.Abstractions;

namespace Dorbit.Framework.Contracts.Identities;

public class IdentityUserDto : IUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public Guid GetId()
    {
        return Id;
    }

    public string GetUsername()
    {
        return Username;
    }

    public bool IsActive()
    {
        return true;
    }
}