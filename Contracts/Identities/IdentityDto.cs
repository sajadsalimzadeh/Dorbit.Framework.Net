using System.Collections.Generic;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Extensions;

namespace Dorbit.Framework.Contracts.Identities;

public class IdentityDto
{
    public IUserDto User { get; set; }
    public bool IsFullAccess { get; set; }
    public List<string> Accessibility { get; set; }
    public List<ClaimDto> Claims { get; set; }
    public HashSet<string> DeepAccessibility { get; set; }

    public bool HasAccess(string access)
    {
        return IsFullAccess || access.IsNullOrEmpty() || DeepAccessibility.Contains(access);
    }
}