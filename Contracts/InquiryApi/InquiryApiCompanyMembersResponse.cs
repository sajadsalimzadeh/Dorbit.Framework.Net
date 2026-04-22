using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiCompanyMembersResponse
{
    public List<InquiryApiCompanyMembersResponseBoardMember> BoardMembers { get; set; }
    public List<InquiryApiCompanyMembersResponseShareHolder> ShareHolders { get; set; }
}

public class InquiryApiCompanyMembersResponseBoardMember
{
    [JsonPropertyName("nationalID")]
    public string NationalId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int RoleCode { get; set; }
    public string RoleName { get; set; }
}

public class InquiryApiCompanyMembersResponseShareHolder
{
    [JsonPropertyName("nationalID")]
    public string NationalId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}