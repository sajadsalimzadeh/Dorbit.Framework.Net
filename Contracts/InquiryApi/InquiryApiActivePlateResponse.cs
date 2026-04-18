namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiActivePlateResponse
{
    public string NationalCode { get; set; }
    public string PlateNumber { get; set; }
    public bool Revoked { get; set; }
    public string RevokedDate { get; set; }
    public string RevokedDescription { get; set; }
    public string SerialNumber { get; set; }
}