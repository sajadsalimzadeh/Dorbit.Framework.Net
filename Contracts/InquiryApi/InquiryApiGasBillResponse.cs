using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiGasBillResponse
{
    public long Amount { get; set; }
    [JsonPropertyName("billID")]
    public string BillId { get; set; }

    [JsonPropertyName("payID")]
    public string PayId { get; set; }

    public string Date { get; set; }
}