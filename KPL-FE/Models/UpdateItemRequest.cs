using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class UpdateItemRequest
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
