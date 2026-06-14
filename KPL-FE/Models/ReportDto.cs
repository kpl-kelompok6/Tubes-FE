using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class ReportDto
{
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; init; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; init; }

    [JsonPropertyName("totalTransactions")]
    public int TotalTransactions { get; init; }

    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; init; }

    [JsonPropertyName("averageTransaction")]
    public decimal AverageTransaction { get; init; }

    [JsonPropertyName("breakdown")]
    public List<PaymentBreakdownDto> Breakdown { get; init; } = [];

    public string TotalRevenueFormatted => $"Rp {TotalRevenue:N0}";
    public string AverageTransactionFormatted => $"Rp {AverageTransaction:N0}";
}

public sealed class PaymentBreakdownDto
{
    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; init; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("total")]
    public decimal Total { get; init; }

    public string TotalFormatted => $"Rp {Total:N0}";
    public string PaymentMethodDisplay => PaymentMethod switch
    {
        "Cash" => "Tunai",
        "Debit" => "Debit",
        "QRIS" => "QRIS",
        "Transfer" => "Transfer",
        _ => PaymentMethod
    };
}
