using System.IO;

namespace Dorbit.Models.Messages;

public class MessageEmailRequest : MessageRequest
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public string To { get; set; }
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