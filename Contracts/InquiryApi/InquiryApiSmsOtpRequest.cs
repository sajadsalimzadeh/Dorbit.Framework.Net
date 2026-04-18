namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiSmsOtpRequest
{
    public string Code { get; set; }
    public string Mobile { get; set; }
    public int Template { get; set; }
}