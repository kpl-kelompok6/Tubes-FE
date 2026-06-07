using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class CreateTransactionRequest
{
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("tableNumber")]
    public string? TableNumber { get; set; }
}
