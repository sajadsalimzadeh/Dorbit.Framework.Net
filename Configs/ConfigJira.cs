using System.Collections.Generic;

namespace Dorbit.Framework.Configs;

public class ConfigJira : ConfigClientApi
{
    public string Username { get; set; } //Jira user email
    public Dictionary<string, int> CustomFields { get; set; }
}