namespace Dorbit.Framework.Contracts.Messages;

public abstract class MessageRequest
{
    public string Receiver { get; set; }
    public string ProviderName { get; set; }
    public string TemplateType { get; set; }
    public string TemplateId { get; set; }
    public string Body { get; set; }
}