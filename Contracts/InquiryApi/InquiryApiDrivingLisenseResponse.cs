using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiDrivingLisenseResponse
{
    public List<InquiryApiDrivingLisenseResponseLicense> Licenses { get; set; }
}

public class InquiryApiDrivingLisenseResponseLicense
{
    public string NationalCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Title { get; set; }
    public string RequestDate { get; set; }
    public string ConfirmDate { get; set; }
    public string PrintDate { get; set; }
    public string PostalBarcode { get; set; }
    public string RahvarStatus { get; set; }
    public string LicenseNumber { get; set; }
    public string ValidYears { get; set; }
}