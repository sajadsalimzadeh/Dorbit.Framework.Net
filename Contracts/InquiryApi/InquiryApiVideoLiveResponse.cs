namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiVideoLiveResponse
{
    public int MatchingScore { get; set; }
    public int LivenessScore { get; set; }
    public bool IsMatch { get; set; }
    public bool IsLiveness { get; set; }
}