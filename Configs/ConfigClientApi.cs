using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Configs;

public abstract class ConfigClientApi
{
    public string ApiUrl { get; set; }
    public ProtectedProperty ApiKey { get; set; }
}