using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Configs;

public class ConfigOpenAi
{
    public ProtectedProperty ApiKey { get; set; }
    public string Model { get; set; } = "gpt-5-nano";
}