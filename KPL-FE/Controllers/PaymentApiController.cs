using KPL_FE.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class PaymentApiController
{
    private static readonly HttpClient _http = new();
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string Url(string path) => $"{App.BaseUrl.TrimEnd('/')}/api/{path}";

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await _http.PostAsync(Url("payments"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        
        var json = await resp.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<PaymentResponse>>(json, _json);
        if (wrapper is null || !wrapper.Success)
            throw new Exception(wrapper?.Message ?? "Gagal memproses pembayaran.");
        
        return wrapper.Data!;
    }
}
