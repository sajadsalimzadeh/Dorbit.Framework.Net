namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiChequeInfoResponse
{
    public string Iban { get; set; }
    public string IssueDate { get; set; }
    public string ExpirationDate { get; set; }
    public string SerialNumber { get; set; }
    public string SeriesNumber { get; set; }
    public string ChequeType { get; set; }
    public string BranchCode { get; set; }
    public string Name { get; set; }
}