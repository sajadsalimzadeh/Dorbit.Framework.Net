using System.Collections.Generic;
using Dorbit.Framework.Contracts.Messages;

namespace Dorbit.Framework.Configs;

public class ConfigMessageProviders
{
    public List<ConfigMessageSmsProvider> Sms { get; set; }
    public List<ConfigMessageEmailProvider> Email { get; set; }
    public List<ConfigMessageNotificationProvider> Notification { get; set; }
}

public class ConfigMessageProvider
{
    public string Name { get; set; }
    public Dictionary<string, string> Templates { get; set; }
}