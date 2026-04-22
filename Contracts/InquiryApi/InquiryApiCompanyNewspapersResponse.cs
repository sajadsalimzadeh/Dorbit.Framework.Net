using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCompanyNewspapersResponse
{
    [JsonPropertyName("newsID")]
    public int NewsId { get; set; }

    public string Title { get; set; }
    
    [JsonPropertyName("nationalID")]
    public string NationalId { get; set; }

    public string Description { get; set; }
    public int Capital { get; set; }
    public string PublicationDate { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public int Page { get; set; }
    public string LetterDate { get; set; }
    public string LetterNumber { get; set; }
}