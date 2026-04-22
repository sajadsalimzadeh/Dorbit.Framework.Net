namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCardMatchRequest
{
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    public string CardNumber { get; set; }
}