using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCompanyInfoResponse
{
    public string CompanyType { get; set; }
    public string Name { get; set; }
    [JsonPropertyName("nationalID")]
    public int NationalId { get; set; }

    public int RegisterNumber { get; set; }
    public string RegisterDate { get; set; }
    public bool Active { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string Provice { get; set; }
    public string City { get; set; }
    public string EndDate { get; set; }
}