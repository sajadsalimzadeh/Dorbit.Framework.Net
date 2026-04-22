using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Identities;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Services;

[ServiceRegister(Order = -1)]
internal class IdentityService(JwtService jwtService, IOptions<ConfigIdentity> configIdentityOptions) : IIdentityService
{
    private readonly ConfigIdentity _configIdentity = configIdentityOptions.Value;
    public IdentityDto Identity { get; private set; }

    public Task<IdentityDto> ValidateAsync(IdentityRequest request)
    {
        if (!jwtService.TryValidateToken(request.AccessToken, _configIdentity.Secret.GetDecryptedValue(), out var token, out var claimsPrincipal))
            throw new AuthenticationException();

        Identity = new IdentityDto()
        {
            Accessibility = claimsPrincipal.Claims.Where(x => x.Type == "access").Select(x => x.Value.ToLower()).ToList(),
            Claims = claimsPrincipal.Claims.Select(x => new IdentityClaimDto(x.Type, x.Value)).ToList(),
            User = new IdentityUserDto
            {
                Id = Guid.Parse(claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "Id")?.Value ?? throw new Exception("Claim id not set in token")),
                Username = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value,
            }
        };

        if (claimsPrincipal.Claims.Any(x => x is { Type: "access", Value: "full" }))
            Identity.IsFullAccess = true;

        Identity.DeepAccessibility = Identity.Accessibility.ToHashSet();
        
        return Task.FromResult(Identity);
    }
}