using System.Text.Json.Serialization;

namespace KPL_FE.Models;

public sealed class RegisterRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = "Kasir";
}
