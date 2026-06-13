namespace KPL_FE.Models;

public sealed class AppConfig
{
    public string BaseUrl { get; set; } = "";
    public string? Token { get; set; }
    public int EmployeeId { get; set; }
    public string? DisplayName { get; set; }
    public string? Role { get; set; }
}
