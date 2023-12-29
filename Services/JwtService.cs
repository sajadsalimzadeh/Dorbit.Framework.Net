using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Models.Jwts;
using Microsoft.IdentityModel.Tokens;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class JwtService
{
    public Task<AuthCreateTokenResponse> CreateToken(AuthCreateTokenRequest request)
    {
        var secret = App.Setting.Security.Secret.GetDecryptedValue();
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Expires = request.Expires,
            Claims = new Dictionary<string, object>(),
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
        var key = tokenHandler.WriteToken(token);

        return Task.FromResult(new AuthCreateTokenResponse()
        {
            Key = key
        });
    }

    public Task<bool> TryValidateTokenAsync(string token)
    {
        return TryValidateTokenAsync(token, out _, out _);
    }

    public Task<bool> TryValidateTokenAsync(string token, out SecurityToken securityToken, out ClaimsPrincipal claimsPrincipal)
    {
        var secret = App.Setting.Security.Secret.GetDecryptedValue();
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
            return Task.FromResult(securityToken.ValidTo > DateTime.UtcNow);
        }
        catch
        {
            // ignored
        }

        claimsPrincipal = null;
        securityToken = null;
        return Task.FromResult(false);
    }
}