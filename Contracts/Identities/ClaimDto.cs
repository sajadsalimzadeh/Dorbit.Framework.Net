using System.ComponentModel.DataAnnotations.Schema;

namespace Dorbit.Framework.Contracts.Identities;

[NotMapped]
public class ClaimDto
{
    public string Type { get; set; }
    public string Value { get; set; }
}