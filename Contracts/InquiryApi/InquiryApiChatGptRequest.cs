namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiChatGptRequest
{
    public string Command { get; set; }
    public string Data { get; set; }
    public double Temperature { get; set; }
}