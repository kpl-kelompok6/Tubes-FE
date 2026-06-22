using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class HealthChecker : IDisposable
{
    private readonly HttpClient _http;

    public HealthChecker()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task<(bool ok, string error)> VerifyAsync(string baseUrl)
    {
        var url = baseUrl.TrimEnd('/') + "/health/live";

        try
        {
            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                return (false, $"Server returned {(int)resp.StatusCode} {resp.ReasonPhrase}");

            var body = await resp.Content.ReadAsStringAsync();

            var doc = JsonSerializer.Deserialize<JsonElement>(body);

            if (!doc.TryGetProperty("success", out var successProp) || successProp.ValueKind != JsonValueKind.True)
                return (false, "Response missing 'success: true'. Is this the correct backend?");

            return (true, null!);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Cannot reach server: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timed out. Check URL and try again.");
        }
        catch (JsonException)
        {
            return (false, "Invalid JSON response. Is this the correct backend?");
        }
        catch (InvalidOperationException ex)
        {
            return (false, $"Invalid URL format: {ex.Message}");
        }
        catch (UriFormatException ex)
        {
            return (false, $"Invalid URL format: {ex.Message}");
        }
    }

    public void Dispose() => _http.Dispose();
}
