using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiPlateHistoryResponse
{
    public string Description { get; set; }
    public string SerialNumber { get; set; }
    public List<InquiryApiPlateHistoryResponseItem> PlateHistory { get; set; }
}

public class InquiryApiPlateHistoryResponseItem
{
    public string VehicleSystem { get; set; }
    public string VehicleType { get; set; }
    public string InstallDate { get; set; }
    public string DetachDate { get; set; }
    public string VehicleModel { get; set; }
}