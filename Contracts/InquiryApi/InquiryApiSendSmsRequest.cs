using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiSendSmsRequest
{
    public string Message { get; set; }
    [JsonPropertyName("mobiles")]
    public List<string> Numbers { get; set; }
}