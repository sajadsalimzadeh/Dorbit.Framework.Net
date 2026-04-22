using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCallRequest
{
    [JsonPropertyName("voiceID")]
    public string VoiceId { get; set; }

    public List<string> Numbers { get; set; }
}