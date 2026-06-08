using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class TransactionApiController
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string Url(string path) => $"{App.BaseUrl.TrimEnd('/')}/api/{path}";

    public async Task<List<TransactionDto>> GetAllAsync()
    {
        var resp = await _http.GetAsync(Url("transactions"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<List<TransactionDto>>(resp) ?? [];
    }

    public async Task<TransactionDto> GetByIdAsync(int id)
    {
        var resp = await _http.GetAsync(Url($"transactions/{id}"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<TransactionDto>(resp);
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await _http.PostAsync(Url("transactions"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<TransactionDto>(resp);
    }

    public async Task<TransactionDto> AddItemAsync(int transactionId, AddItemRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var resp = await _http.PostAsync(Url($"transactions/{transactionId}/items"), new StringContent(body, null, "application/json"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<TransactionDto>(resp);
    }

    public async Task<TransactionDto> RemoveItemAsync(int transactionId, int itemId)
    {
        var resp = await _http.DeleteAsync(Url($"transactions/{transactionId}/items/{itemId}"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<TransactionDto>(resp);
    }

    public async Task<TransactionDto> UpdateItemQuantityAsync(int transactionId, int itemId, UpdateItemRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), Url($"transactions/{transactionId}/items/{itemId}"))
        {
            Content = new StringContent(body, null, "application/json")
        };
        var resp = await _http.SendAsync(requestMessage);
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<TransactionDto>(resp);
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
