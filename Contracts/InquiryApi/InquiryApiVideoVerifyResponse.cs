namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiVideoVerifyResponse
{
    public int MatchingScore { get; set; }
    public int LivenessScore { get; set; }
    public int SpeechScore { get; set; }
    public bool IsMatch { get; set; }
    public bool IsLiveness { get; set; }
    public bool IsSpeechMatched { get; set; }
}