using Dorbit.Framework.Contracts.Cryptograpy;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;
using Serilog;

namespace Dorbit.Framework.Configs;

public abstract class ConfigClientApi
{
    public string ApiUrl { get; set; }
    public string BaseUrl { get; set; }
    public ProtectedProperty ApiKey { get; set; }
}