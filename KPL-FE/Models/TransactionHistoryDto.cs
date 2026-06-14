using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class TransactionHistoryDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("transactionId")]
    public int TransactionId { get; init; }

    [JsonPropertyName("transactionCode")]
    public string TransactionCode { get; init; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; init; }

    [JsonPropertyName("tableNumber")]
    public string? TableNumber { get; init; }

    [JsonPropertyName("paymentMethod")]
    public string PaymentMethod { get; init; } = string.Empty;

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; init; }

    [JsonPropertyName("paidAmount")]
    public decimal PaidAmount { get; init; }

    [JsonPropertyName("change")]
    public decimal Change { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("items")]
    public List<TransactionItemDto> Items { get; init; } = [];

    public string TotalAmountFormatted => $"Rp {TotalAmount:N0}";
    public string PaidAmountFormatted => $"Rp {PaidAmount:N0}";
    public string ChangeFormatted => $"Rp {Change:N0}";
    public string DateFormatted => CreatedAt.ToString("dd/MM/yyyy");
    public string TimeFormatted => CreatedAt.ToString("HH:mm");
    public string PaymentMethodDisplay => PaymentMethod switch
    {
        "Cash" => "Tunai",
        "Debit" => "Debit",
        "QRIS" => "QRIS",
        "Transfer" => "Transfer",
        _ => PaymentMethod
    };
}
