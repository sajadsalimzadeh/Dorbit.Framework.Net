using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Jwts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Dorbit.Framework.Services;

[ServiceRegister(Lifetime = ServiceLifetime.Singleton)]
public class JwtService()
{

    public string CreateToken(JwtCreateTokenRequest request)
    {
        var secret = request.Secret;
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject  = request.Claims,
            Issuer = request.Issuer,
            Audience = request.Audience,
            Expires = request.ExpireAt,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool TryValidateToken(string token, string secret)
    {
        return TryValidateToken(token, secret, out _, out _);
    }

    public bool TryValidateToken(string token, string secret, out SecurityToken securityToken, out ClaimsPrincipal principals)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
            };
            principals = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            return securityToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            // ignored
        }

        principals = null;
        securityToken = null;
        return false;
    }
}