using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiChequeInfoRequest
{
    [JsonPropertyName("chequeID")]
    public string ChequeId { get; set; }
}