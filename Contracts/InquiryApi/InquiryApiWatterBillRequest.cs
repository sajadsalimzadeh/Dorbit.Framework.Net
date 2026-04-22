using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiWatterBillRequest
{
    [JsonPropertyName("BillID")]
    public string BillId { get; set; }
}