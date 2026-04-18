namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiVehicleCardResponse
{
    public string CardPostalBarcode { get; set; }
    public InquiryApiVehicleCardResponseCardStatus CardStatus { get; set; }
    public string CardIssuanceDate { get; set; }
    public string CardPrintDate { get; set; }
    public bool IsSmart { get; set; }
    public InquiryApiVehicleCardResponseCardType CardType { get; set; }
    public int DocumentStatus { get; set; }
    public string DocumentIssuanceDate { get; set; }
    public string DocumentPrintDate { get; set; }
    public InquiryApiVehicleCardResponseDocumentType DocumentType { get; set; }
}

public class InquiryApiVehicleCardResponseCardStatus
{
    public int Id { get; set; }
    public string Description { get; set; }
}

public class InquiryApiVehicleCardResponseCardType
{
    public int Id { get; set; }
    public string Description { get; set; }
}

public class InquiryApiVehicleCardResponseDocumentType
{
    public int Id { get; set; }
    public string Description { get; set; }
}