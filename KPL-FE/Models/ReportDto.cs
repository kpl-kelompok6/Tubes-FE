using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class ReportDto
{
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; init; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; init; }

    [JsonPropertyName("totalTransaksi")]
    public int TotalTransactions { get; init; }

    [JsonPropertyName("totalPendapatan")]
    public decimal TotalRevenue { get; init; }

    [JsonPropertyName("rataRata")]
    public decimal AverageTransaction { get; init; }

    [JsonPropertyName("breakdown")]
    public Dictionary<string, decimal> BreakdownRaw { get; init; } = [];

    [JsonIgnore]
    public List<PaymentBreakdownDisplay> Breakdown => BreakdownRaw
        .Select(kvp => new PaymentBreakdownDisplay(kvp.Key, kvp.Value))
        .ToList();

    public string TotalRevenueFormatted => $"Rp {TotalRevenue:N0}";
    public string AverageTransactionFormatted => $"Rp {AverageTransaction:N0}";
}

public sealed class PaymentBreakdownDisplay
{
    public string PaymentMethodRaw { get; }
    public decimal Total { get; }
    public string TotalFormatted => $"Rp {Total:N0}";
    public string PaymentMethodDisplay => PaymentMethodRaw switch
    {
        "cash" => "Tunai",
        "debit" => "Debit",
        "qris" => "QRIS",
        "transfer" => "Transfer",
        _ => PaymentMethodRaw
    };

    public PaymentBreakdownDisplay(string paymentMethod, decimal total)
    {
        PaymentMethodRaw = paymentMethod;
        Total = total;
    }
}
