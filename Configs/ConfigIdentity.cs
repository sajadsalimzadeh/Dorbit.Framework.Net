using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Configs;

public class ConfigIdentity
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public ProtectedProperty Secret { get; set; }
}