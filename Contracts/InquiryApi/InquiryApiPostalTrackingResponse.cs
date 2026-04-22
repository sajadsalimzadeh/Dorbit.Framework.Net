using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiPostalTrackingResponse
{
    public string PostType { get; set; }
    public string SourcePostOffice { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public string SenderName { get; set; }
    public string ReceiveName { get; set; }
    public string SourcePostalCode { get; set; }
    public string DestinationPostalCode { get; set; }
    public string Weight { get; set; }
    public string TotalAmount { get; set; }
    public List<InquiryApiPostalTrackingResponseDetail> Details { get; set; }
}

public class InquiryApiPostalTrackingResponseDetail
{
    public string Date { get; set; }
    public string Event { get; set; }
    public string Id { get; set; }
    public string PostalNode { get; set; }
    public string Time { get; set; }
}