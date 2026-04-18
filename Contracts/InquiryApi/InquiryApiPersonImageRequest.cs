namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiPersonImageRequest
{
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    
    /// <summary>
    /// سریال پشت کارت ملی یا رهیگیری رسید کارت ملی
    /// </summary>
    public string SerialNumber { get; set; }
}