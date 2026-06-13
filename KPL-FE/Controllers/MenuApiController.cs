using KPL_FE.Models;
using System;
using System.Net.Http;
using System.Text.Json;

namespace KPL_FE.Controllers;

public sealed class MenuApiController
{
    private static readonly HttpClient _http = App.ApiHttp;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string Url(string path) => $"{App.BaseUrl.TrimEnd('/')}/api/{path}";

    public async Task<List<MenuDto>> GetAllAsync()
    {
        var resp = await _http.GetAsync(Url("menus"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<List<MenuDto>>(resp) ?? [];
    }

    public async Task<MenuDto> AddAsync(MenuRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await _http.PostAsync(Url("menus"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<MenuDto>(resp);
    }

    public async Task<MenuDto> UpdateAsync(int id, MenuRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await _http.PutAsync(Url($"menus/{id}"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<MenuDto>(resp);
    }

    public async Task DeleteAsync(int id)
    {
        var resp = await _http.DeleteAsync(Url($"menus/{id}"));
        resp.EnsureSuccessStatusCode();
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
