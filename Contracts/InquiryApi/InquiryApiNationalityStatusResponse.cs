using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiNationalityStatusResponse
{
    public int Id { get; set; }
    public long UniqueCode { get; set; }
    public long FidaCode { get; set; }
    public string BirthDate { get; set; }
    public string BirthDatePersian { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FatherName { get; set; }
    public string GrandFatherName { get; set; }
    public int Gender { get; set; }

    [JsonPropertyName("ProvinceID")]
    public int ProvinceId { get; set; }

    public string Province { get; set; }

    [JsonPropertyName("nationalityID")]
    public long NationalityId { get; set; }

    public string NationalityName { get; set; }
    public int Status { get; set; }

    [JsonPropertyName("familyID")]
    public long FamilyId { get; set; }

    public bool Exit { get; set; }
    public long IdentityCode { get; set; }
    public long Relative { get; set; }
    public long Education { get; set; }
    public bool IsActive { get; set; }
    public string DeleteAt { get; set; }
}