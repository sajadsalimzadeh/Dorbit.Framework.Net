namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiFaceMatchRequest
{
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    public string SerialNumber { get; set; }
    public string ImageBase64 { get; set; }
    public int MatchingThreshold { get; set; }
}