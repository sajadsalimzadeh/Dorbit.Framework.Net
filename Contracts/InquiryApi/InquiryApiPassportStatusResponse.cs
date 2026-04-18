namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiPassportStatusResponse
{
    public bool HasRequest { get; set; }
    public string RequestStatus { get; set; }
    public string RequestDate { get; set; }
    public string PostalTrackingCode { get; set; }
    public bool HasPassport { get; set; }
    public string PassportNumber { get; set; }
    public string IssueDate { get; set; }
    public string ExpirationDate { get; set; }
    public string PassportStatus { get; set; }
    public bool PersonFound { get; set; }
}