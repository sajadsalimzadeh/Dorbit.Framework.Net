using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCompanyMembersRequest
{
    [JsonPropertyName("nationalID")]
    public string NationalId { get; set; }
}