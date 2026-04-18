namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiVehicleViolationResponse
{
    
}

public class InquiryApiVehicleViolationResponseItem
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public long Price { get; set; }
    public string City { get; set; }
    public string Location { get; set; }
    public string Serial { get; set; }
    public string DataValue { get; set; }
    public string Barcode { get; set; }
    public string License { get; set; }
    public string BillId { get; set; }
    public string PaymentId { get; set; }
    public string Date { get; set; }
    public string DateEn { get; set; }
    public bool IsPayable { get; set; }
    public string PolicemanCode { get; set; }
    public bool HasImage { get; set; }
}