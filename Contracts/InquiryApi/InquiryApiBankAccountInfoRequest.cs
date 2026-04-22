namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiBankAccountInfoRequest
{
    public string AccountNumber { get; set; }
    public InquiryApiBankCode BankCode { get; set; }
}