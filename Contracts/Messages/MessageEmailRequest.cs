using System.Collections.Generic;
using System.IO;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Cryptograpy;

namespace Dorbit.Framework.Contracts.Messages;

public class MessageEmailRequest : MessageRequest
{
    public string Subject { get; set; }
    public string Cc { get; set; }
    public string Bcc { get; set; }
    public List<MessageEmailRequestAttachment> Attachments { get; set; }
    public object[] Args { get; set; }
}

public class MessageEmailRequestAttachment
{
    public string Name { get; set; }
    public string ContentType { get; set; }
    public Stream Stream { get; set; }
}

public class ConfigMessageEmailProvider : ConfigMessageProvider
{
    public string SenderName { get; set; }
    public string Sender { get; set; }
    public string Username { get; set; }
    public ProtectedProperty Password { get; set; }
    public ProtectedProperty ApiKey { get; set; }
    public string Server { get; set; }
    public short Port { get; set; }
}