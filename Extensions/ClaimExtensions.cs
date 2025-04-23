using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Dorbit.Framework.Extensions;

public static class ClaimExtensions
{
    public static string Get(this IEnumerable<Claim> claims, string type)
    {
        return claims.FirstOrDefault(x => x.Type == type)?.Value;
    }
    
    public static bool GetBoolean(this Claim claim)
    {
        return claim is not null && Convert.ToBoolean(claim.Value);
    }
    
    public static bool GetBoolean(this IEnumerable<Claim> claims, string type)
    {
        return claims.FirstOrDefault(x => x.Type == type).GetBoolean();
    }
    
    public static bool TryGetInt32(this Claim claim, out int value)
    {
        value = 0;
        if (claim is null) return false;
        try
        {
            value = Convert.ToInt32(claim.Value);
        }
        catch
        {
            // ignored
        }

        return true;
    }
    
    public static bool TryGetInt32(this IEnumerable<Claim> claims, string type, out int value)
    {
        return claims.FirstOrDefault(x => x.Type == type).TryGetInt32(out value);
    }
    
    public static bool TryGetGuid(this Claim claim, out Guid value)
    {
        value = Guid.Empty;
        if (claim is null) return false;
        if (!Guid.TryParse(claim.Value, out var v)) return false;
        value = v;
        return true;
    }

    public static bool TryGetGuid(this IEnumerable<Claim> claims, string type, out Guid value)
    {
        return claims.FirstOrDefault(x => x.Type == type).TryGetGuid(out value);
    }
}