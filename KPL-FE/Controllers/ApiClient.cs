using KPL_FE.Models;
using System.Net.Http;
using System.Text.Json;

namespace KPL_FE.Controllers;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string Url(string path) => $"{App.BaseUrl.TrimEnd('/')}/api/{path}";

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T> GetAsync<T>(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Url(path));
        var resp = await _http.SendAsync(request);
        return await HandleResponseAsync<T>(resp);
    }

    public async Task<T> PostAsync<T>(string path, object body)
    {
        var json = JsonSerializer.Serialize(body, _json);
        using var request = new HttpRequestMessage(HttpMethod.Post, Url(path))
        {
            Content = new StringContent(json, null, "application/json")
        };
        var resp = await _http.SendAsync(request);
        return await HandleResponseAsync<T>(resp);
    }

    public async Task<T> PutAsync<T>(string path, object body)
    {
        var json = JsonSerializer.Serialize(body, _json);
        using var request = new HttpRequestMessage(HttpMethod.Put, Url(path))
        {
            Content = new StringContent(json, null, "application/json")
        };
        var resp = await _http.SendAsync(request);
        return await HandleResponseAsync<T>(resp);
    }

    public async Task<T> PatchAsync<T>(string path, object body)
    {
        var json = JsonSerializer.Serialize(body, _json);
        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), Url(path))
        {
            Content = new StringContent(json, null, "application/json")
        };
        var resp = await _http.SendAsync(request);
        return await HandleResponseAsync<T>(resp);
    }

    public async Task DeleteAsync(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, Url(path));
        var resp = await _http.SendAsync(request);
        await ThrowOnErrorAsync(resp);
    }

    public async Task<T> DeleteAsync<T>(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, Url(path));
        var resp = await _http.SendAsync(request);
        return await HandleResponseAsync<T>(resp);
    }

    private static async Task<T> HandleResponseAsync<T>(HttpResponseMessage resp)
    {
        await ThrowOnErrorAsync(resp);
        return await ParseDataAsync<T>(resp);
    }

    private static async Task ThrowOnErrorAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        var json = await resp.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                var first = errors.EnumerateArray()
                    .Select(e => e.GetString())
                    .FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));
                if (!string.IsNullOrWhiteSpace(first))
                    throw new Exception(first);
            }

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                var value = message.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                    throw new Exception(value);
            }
        }
        catch (JsonException) { }

        throw new Exception($"Server returned {(int)resp.StatusCode} {resp.ReasonPhrase}");
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
