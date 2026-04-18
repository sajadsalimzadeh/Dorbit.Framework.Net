namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiVideoVerifyRequest
{
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    public string SerialNumber { get; set; }
    public string VideoBase64 { get; set; }
    public string SpeechText { get; set; }
    public int MatchingThreshold { get; set; }
    public int LivenessThreshold { get; set; }
    public int SpeechThreshold { get; set; }
}