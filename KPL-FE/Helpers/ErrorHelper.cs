using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KPL_FE.Helpers;

public static class ErrorHelper
{
    public static string GetFriendlyErrorMessage(Exception ex, string context)
    {
        if (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
            return $"Server tidak merespons saat memuat {context}. Coba Lagi.";

        if (ex is HttpRequestException)
            return $"Tidak dapat terhubung ke server saat memuat {context}. Coba Lagi.";

        return $"Gagal memuat {context}: {ex.Message}";
    }
}
