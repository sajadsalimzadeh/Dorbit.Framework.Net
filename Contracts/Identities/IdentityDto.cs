using System.Collections.Generic;
using Dorbit.Framework.Contracts.Abstractions;
using Newtonsoft.Json;

namespace Dorbit.Framework.Contracts.Identities;

public class IdentityDto
{
    public IUserDto User { get; set; }
    public bool IsAdmin { get; set; }
    public HashSet<string> Accessibility { get; set; }

    public bool HasAccess(string access)
    {
        return IsAdmin || Accessibility.Contains(access);
    }
}