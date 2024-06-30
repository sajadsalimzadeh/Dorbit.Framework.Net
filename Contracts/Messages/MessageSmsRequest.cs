using System.Collections.Generic;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Contracts.Messages;

public class MessageSmsRequest : MessageRequest
{
    public string To { get; set; }
    public string Text { get; set; }
    public string[] Args { get; set; }
}

public class ConfigMessageSmsProvider : ConfigMessageProvider
{
    public string Sender { get; set; }
    public string Username { get; set; }
    public ProtectedProperty ApiKey { get; set; }
    public ProtectedProperty Password { get; set; }
}