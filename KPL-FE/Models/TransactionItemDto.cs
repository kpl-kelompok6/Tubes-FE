using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class TransactionItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("menuId")]
    public int MenuId { get; init; }

    [JsonPropertyName("menuName")]
    public string MenuName { get; init; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; init; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; init; }

    public string UnitPriceFormatted => $"Rp {UnitPrice:N0}";
    public string SubtotalFormatted => $"Rp {Subtotal:N0}";
}
