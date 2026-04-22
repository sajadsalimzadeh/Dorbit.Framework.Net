using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts.Identities;

[NotMapped]
public class IdentityClaimDto(string type, string value)
{
    public string Type { get; } = type;
    public string Value { get; } = value;
}