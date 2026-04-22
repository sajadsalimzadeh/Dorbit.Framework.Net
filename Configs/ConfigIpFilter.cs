using System.Collections.Generic;

namespace Dorbit.Framework.Configs;

public class ConfigIpFilter
{
    public Dictionary<string, List<string>> Groups { get; set; } = [];
}