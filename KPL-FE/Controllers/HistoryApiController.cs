using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class HistoryApiController
{
    private static readonly HttpClient _http = App.ApiHttp;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string Url(string path) => $"{App.BaseUrl.TrimEnd('/')}/api/{path}";

    public async Task<List<TransactionHistoryDto>> GetAllAsync()
    {
        var resp = await _http.GetAsync(Url("histories"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<List<TransactionHistoryDto>>(resp) ?? [];
    }

    public async Task<TransactionHistoryDto> GetByIdAsync(int id)
    {
        var resp = await _http.GetAsync(Url($"histories/{id}"));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<TransactionHistoryDto>(resp);
    }

    public async Task<List<TransactionHistoryDto>> GetFilteredAsync(DateTime? start, DateTime? end)
    {
        var qs = BuildQueryString(start, end);
        var path = string.IsNullOrEmpty(qs) ? "histories/filter" : $"histories/filter?{qs}";
        var resp = await _http.GetAsync(Url(path));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<List<TransactionHistoryDto>>(resp) ?? [];
    }

    public async Task<ReportDto> GetReportAsync(DateTime? start, DateTime? end)
    {
        var qs = BuildQueryString(start, end);
        var path = string.IsNullOrEmpty(qs) ? "histories/report" : $"histories/report?{qs}";
        var resp = await _http.GetAsync(Url(path));
        resp.EnsureSuccessStatusCode();
        return await ParseDataAsync<ReportDto>(resp);
    }

    private static string BuildQueryString(DateTime? start, DateTime? end)
    {
        var parts = new List<string>();
        if (start.HasValue)
            parts.Add($"start={start.Value:yyyy-MM-dd}");
        if (end.HasValue)
            parts.Add($"end={end.Value:yyyy-MM-dd}");
        return string.Join("&", parts);
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
