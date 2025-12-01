using System.Collections.Generic;

namespace Dorbit.Framework.Configs;

public class ConfigFile
{
    public string BasePath { get; set; }
    public int MaxSize { get; set; } = 5 * 1024 * 1024;
    public Dictionary<string, int> MaxSizeAccessibility { get; set; }
}