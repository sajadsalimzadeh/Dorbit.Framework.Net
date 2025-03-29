using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Settings;

public class SettingGetRequest
{
    public List<string> Keys { get; set; }
}