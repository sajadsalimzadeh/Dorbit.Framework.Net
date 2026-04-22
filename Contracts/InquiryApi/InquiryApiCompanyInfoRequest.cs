using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCompanyInfoRequest
{
    [JsonPropertyName("nationalID")]
    public string NationalId { get; set; }
}