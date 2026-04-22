using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCompanySignatoryRequest
{
    public string AllowedTopics { get; set; }
    public InquiryApiCompanySignatoryRequestSignHolders SignHolders { get; set; }
    
}

public class InquiryApiCompanySignatoryRequestSignHolders
{
    public List<InquiryApiCompanySignatoryRequestSignHoldersMember> ObligatorySignature { get; set; }
    public List<InquiryApiCompanySignatoryRequestSignHoldersMember> NormalSignature { get; set; }
    public List<InquiryApiCompanySignatoryRequestSignHoldersMember> ObligatoryAndNormalSignature { get; set; }
}

public class InquiryApiCompanySignatoryRequestSignHoldersMember
{
    public string Name { get; set; }
    public string Title { get; set; }
    public bool ObligatoryStatus { get; set; }
    public bool NormalStatus { get; set; }
    [JsonPropertyName("personID")]
    public string PersonId { get; set; }
}