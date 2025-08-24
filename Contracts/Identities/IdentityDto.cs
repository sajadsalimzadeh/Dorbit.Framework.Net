using System;
using System.Collections.Generic;
using Dorbit.Framework.Contracts.Abstractions;
using Dorbit.Framework.Extensions;
using Newtonsoft.Json;

namespace Dorbit.Framework.Contracts.Identities;

public class IdentityDto
{
    public IUserDto User { get; set; }
    public bool IsFullAccess { get; set; }
    public List<string> Accessibility { get; set; }
    public Dictionary<string, string> Claims { get; set; }
    public HashSet<string> DeepAccessibility { get; set; }

    public bool HasAccess(string access)
    {
        return IsFullAccess || access.IsNullOrEmpty() || DeepAccessibility.Contains(access);
    }
}