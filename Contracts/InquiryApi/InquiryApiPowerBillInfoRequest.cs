using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiPowerBillInfoRequest
{
    [JsonPropertyName("billID")]
    public string BillId { get; set; }
}