using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Jwts;

public class JwtCreateTokenRequest
{
    public DateTime Expires { get; set; }
    public string CsrfToken { get; set; }
    public Dictionary<string, string> Claims { get; set; }
}