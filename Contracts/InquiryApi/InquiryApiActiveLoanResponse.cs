namespace Dorbit.Framework.Contracts.InquiryApi;

public class InquiryApiActiveLoanResponse
{
    public int Count { get; set; }
    public InquiryApiActiveLoanResponseInfo Info { get; set; }
}

public class InquiryApiActiveLoanResponseInfo
{
    public string NationalCode { get; set; }
    public string Name { get; set; }
    public long TotalAmount { get; set; }
    public long DebtTotalAmount { get; set; }
    public long PastExpiredTotalAmount { get; set; }
    public long DeferredTotalAmount { get; set; }
    public long SuspiciousTotalAmount { get; set; }
    public long Dishonored { get; set; }
}