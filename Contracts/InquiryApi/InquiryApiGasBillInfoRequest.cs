using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiGasBillInfoRequest
{
    [JsonPropertyName("billID")]
    public string BillId { get; set; }
}