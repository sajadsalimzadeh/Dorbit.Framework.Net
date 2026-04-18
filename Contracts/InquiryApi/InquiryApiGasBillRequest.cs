using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiGasBillRequest
{
    [JsonPropertyName("billID")]
    public string BillId { get; set; }
}