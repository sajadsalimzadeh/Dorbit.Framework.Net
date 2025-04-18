﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Jwts;
using Dorbit.Framework.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JwtService(IOptions<ConfigFrameworkSecurity> securityOptions)
{
    private readonly ConfigFrameworkSecurity _configFrameworkSecurity = securityOptions.Value;

    public string CreateToken(JwtCreateTokenRequest request)
    {
        var secret = _configFrameworkSecurity.Secret.GetDecryptedValue();
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Claims = new Dictionary<string, object>(),
            Issuer = _configFrameworkSecurity.Issuer,
            Audience = _configFrameworkSecurity.Audience,
            Expires = request.Expires,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };
        if (request.Claims is not null)
        {
            foreach (var claim in request.Claims)
            {
                tokenDescriptor.Claims.Add(claim.Key, claim.Value);
            }
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool TryValidateToken(string token)
    {
        return TryValidateToken(token, out _, out _);
    }

    public bool TryValidateToken(string token, out SecurityToken securityToken, out ClaimsPrincipal claimsPrincipal)
    {
        var secret = _configFrameworkSecurity.Secret.GetDecryptedValue();
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
            claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            return securityToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            // ignored
        }

        claimsPrincipal = null;
        securityToken = null;
        return false;
    }
}