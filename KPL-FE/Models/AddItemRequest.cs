using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class AddItemRequest
{
    [JsonPropertyName("menuId")]
    public int MenuId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
