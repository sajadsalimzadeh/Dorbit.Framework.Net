namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiTextToSpeechRequest
{
    public string Text { get; set; }
    public bool Male { get; set; }
    public InquiryApiTextToSpeechRequestTtsEnglish TtsEnglish { get; set; } = InquiryApiTextToSpeechRequestTtsEnglish.Auto;
}

public enum InquiryApiTextToSpeechRequestTtsEnglish
{
    Auto = 1,
    Internal = 2,
    External = 3,
}