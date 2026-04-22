using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiPowerBillInfoResponse
{
    public string Print { get; set; }
    public long Amount { get; set; }
    [JsonPropertyName("billID")]
    public string BillId { get; set; }

    [JsonPropertyName("payID")]
    public string PayId { get; set; }

    public string Date { get; set; }
    public InquiryApiPowerBillInfoResponseInfo Info { get; set; }
}

public class InquiryApiPowerBillInfoResponseInfo
{
    public string OwnerName { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string UsageType { get; set; }
    public string MeterNumber { get; set; }
    public string FileNumber { get; set; }
    public string City { get; set; }
    public int Capacity { get; set; }
    public string PreviousReadDate { get; set; }
    public string CurrentReadDate { get; set; }
    public int CurrentConsumption { get; set; }
    public int PreviousNumber { get; set; }
    public int CurrentNumber { get; set; }
}