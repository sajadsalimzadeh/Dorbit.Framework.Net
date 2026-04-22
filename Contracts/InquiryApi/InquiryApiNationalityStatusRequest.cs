namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiNationalityStatusRequest
{
    public string Code { get; set; }
    public InquiryApiNationalityStatusRequestType CodeType { get; set; }
}

public enum InquiryApiNationalityStatusRequestType
{
    Identified = 1,
    Fida = 2,
    Faragir = 3,
    Unique = 4
}