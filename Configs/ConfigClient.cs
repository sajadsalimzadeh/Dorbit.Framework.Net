using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Configs;

public abstract class ConfigClient
{
    public string BaseUrl { get; set; }
    public string ApiUrl { get; set; }
    public ProtectedProperty ApiKey { get; set; }
}