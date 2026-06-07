namespace KPL_FE.Models;

public sealed class PaymentRequest
{
    public int TransactionId { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = "cash";
}
