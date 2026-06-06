using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class TransactionDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("transactionCode")]
    public string TransactionCode { get; init; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; init; }

    [JsonPropertyName("tableNumber")]
    public string? TableNumber { get; init; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; init; }

    [JsonPropertyName("paidAmount")]
    public decimal PaidAmount { get; init; }

    [JsonPropertyName("change")]
    public decimal Change { get; init; }

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; init; }

    [JsonPropertyName("items")]
    public List<TransactionItemDto> Items { get; init; } = [];

    // Formatter Helpers
    public string TotalAmountFormatted => $"Rp {TotalAmount:N0}";
    public string SummaryText => string.IsNullOrEmpty(CustomerName) ? $"Meja {TableNumber ?? "-"}" : $"{CustomerName} (Meja {TableNumber ?? "-"})";
    public string DisplayCode => $"#{TransactionCode}";
}
