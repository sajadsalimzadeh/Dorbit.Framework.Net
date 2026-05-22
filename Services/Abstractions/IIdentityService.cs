using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Identities;
using Microsoft.AspNetCore.Http;

namespace Dorbit.Framework.Services.Abstractions;

public interface IIdentityService
{
    IdentityDto Identity { get; }
    Task<IdentityDto> ValidateAsync(IdentityRequest request);
}