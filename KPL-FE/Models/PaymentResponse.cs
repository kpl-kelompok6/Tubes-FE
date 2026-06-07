using System;

namespace KPL_FE.Models;

public sealed class PaymentResponse
{
    public int PaymentId { get; set; }
    public int TransactionId { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string TotalAmountFormatted => $"Rp {TotalAmount:N0}";
    public string PaidAmountFormatted => $"Rp {PaidAmount:N0}";
    public string ChangeAmountFormatted => $"Rp {ChangeAmount:N0}";
}
