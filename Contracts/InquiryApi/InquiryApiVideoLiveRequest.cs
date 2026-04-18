namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiVideoLiveRequest
{
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    public string SerialNumber { get; set; }
    public string VideoBase64 { get; set; }
    public int MatchingThreshold { get; set; }
    public int LivenessThreshold { get; set; }
}