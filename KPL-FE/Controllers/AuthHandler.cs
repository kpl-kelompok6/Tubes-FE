using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using KPL_FE.Views;

namespace KPL_FE.Controllers;

public sealed class AuthHandler : DelegatingHandler
{
    public AuthHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var isAuthRequest = request.RequestUri?.AbsolutePath.Contains("/api/auth/", StringComparison.OrdinalIgnoreCase) == true;

        if (!isAuthRequest && !string.IsNullOrEmpty(App.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);

        var response = await base.SendAsync(request, cancellationToken);

        if (!isAuthRequest && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var dispatcher = Application.Current.Dispatcher;

            string? newToken = null;
            dispatcher.Invoke(() =>
            {
                var login = new Views.LoginPage();
                var owner = Application.Current.MainWindow;
                if (owner is not null)
                    login.Owner = owner;
                login.WindowStartupLocation = owner is null
                    ? WindowStartupLocation.CenterScreen
                    : WindowStartupLocation.CenterOwner;

                if (login.ShowDialog() == true)
                    newToken = App.Token;
            });

            if (!string.IsNullOrEmpty(newToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response.Dispose();
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}
