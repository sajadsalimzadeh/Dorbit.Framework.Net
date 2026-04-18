using System.ComponentModel.DataAnnotations;

namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiIbanMatchRequest
{
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    [MaxLength(26)]
    public string Iban { get; set; }
}