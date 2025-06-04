using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Jwts;

public class JwtCreateTokenRequest(string secret, string csrfToken, DateTime expireAt)
{
    public DateTime ExpireAt { get; set; } = expireAt;
    public string CsrfToken { get; set; } = csrfToken;
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Secret { get; set; } = secret;
    public int TimeoutInSecond { get; set; }
    public Dictionary<string, string> Claims { get; set; }
}