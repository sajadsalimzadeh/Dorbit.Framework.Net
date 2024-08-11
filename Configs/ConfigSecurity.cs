using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Configs;

public class ConfigSecurity
{
    public string Assembly { get; set; }
    public ProtectedProperty Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int TimeoutInSecond { get; set; }
}