using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class HistoryApiController
{
    public async Task<List<TransactionHistoryDto>> GetAllAsync()
        => await App.Api.GetAsync<List<TransactionHistoryDto>>("histories") ?? [];

    public async Task<TransactionHistoryDto> GetByIdAsync(int id)
        => await App.Api.GetAsync<TransactionHistoryDto>($"histories/{id}");

    public async Task<List<TransactionHistoryDto>> GetFilteredAsync(DateTime? start, DateTime? end)
    {
        var qs = BuildQueryString(start, end);
        var path = string.IsNullOrEmpty(qs) ? "histories/filter" : $"histories/filter?{qs}";
        return await App.Api.GetAsync<List<TransactionHistoryDto>>(path) ?? [];
    }

    public async Task<ReportDto> GetReportAsync(DateTime? start, DateTime? end)
    {
        var qs = BuildQueryString(start, end);
        var path = string.IsNullOrEmpty(qs) ? "histories/report" : $"histories/report?{qs}";
        return await App.Api.GetAsync<ReportDto>(path);
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
}
