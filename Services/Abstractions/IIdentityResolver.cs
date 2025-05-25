using Dorbit.Framework.Contracts.Identities;

namespace Dorbit.Framework.Services.Abstractions;

public interface IIdentityResolver
{
    IdentityDto Identity { get; }
}