using KPL_FE.Models;
using System.Net.Http;
using System.Text.Json;

namespace KPL_FE.Controllers;

public sealed class AuthApiController
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string Url(string path) => $"{App.BaseUrl.TrimEnd('/')}/api/{path}";

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await App.ApiHttp.PostAsync(Url("auth/login"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<LoginResponse>(resp);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await App.ApiHttp.PostAsync(Url("auth/register"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<LoginResponse>(resp);
    }

    private static async Task<T> ParseDataAsync<T>(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(json, _json);
        if (wrapper is null || !wrapper.Success)
            throw new Exception(wrapper?.Message ?? "Gagal memuat data dari server.");
        return wrapper.Data!;
    }
}
