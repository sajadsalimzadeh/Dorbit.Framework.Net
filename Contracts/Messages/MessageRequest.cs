namespace Dorbit.Framework.Contracts.Messages;

public abstract class MessageRequest
{
    public string ProviderName { get; set; }
    public string TemplateType { get; set; }
    public string TemplateId { get; set; }
}