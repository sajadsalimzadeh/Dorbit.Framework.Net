using Dorbit.Enums;

namespace Dorbit.Models.Messages;

public class MessageSmsRequest : MessageRequest
{
    public string To { get; set; }
    public string Text { get; set; }
    public string TemplateId { get; set; }
    public object[] Args { get; set; }
}