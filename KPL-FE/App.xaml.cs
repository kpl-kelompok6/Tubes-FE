using KPL_FE.Controllers;
using KPL_FE.Services;
using KPL_FE.Views;
using ModernWpf;
using System.Net.Http;
using System.Windows;

namespace KPL_FE
{
    public partial class App : Application
    {
        public static ConfigController Config { get; } = new();

        public static string BaseUrl { get; internal set; } = string.Empty;

        public static string? Token { get; set; }
        public static int EmployeeId { get; set; }
        public static string? DisplayName { get; set; }
        public static string? Role { get; set; }

        public static HttpClient ApiHttp { get; } = new(new AuthHandler())
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public static ApiClient Api { get; } = new(ApiHttp);

        protected override void OnStartup(StartupEventArgs e)
        {
            // Prevent app from shutting down when the dialog windows (Setup or Login) close
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            DispatcherUnhandledException += async (_, args) =>
            {
                await DialogService.ShowError("Error", args.Exception.Message);
                args.Handled = true;
            };

            var config = Config.Load();

            if (string.IsNullOrEmpty(config.BaseUrl))
            {
                var setup = new SetupWindow { BaseUrl = config.BaseUrl };
                setup.ShowDialog();

                if (!setup.Saved)
                {
                    Shutdown();
                    return;
                }

                config.BaseUrl = setup.BaseUrl;
                Config.Save(config);
            }

            BaseUrl = config.BaseUrl;

            if (config.AppTheme is not null)
            {
                ThemeManager.Current.ApplicationTheme = config.AppTheme switch
                {
                    "Dark" => ApplicationTheme.Dark,
                    _ => ApplicationTheme.Light
                };
            }

            if (string.IsNullOrEmpty(config.Token))
            {
                var login = new LoginPage();
                login.ShowDialog();

                if (!login.Saved)
                {
                    Shutdown();
                    return;
                }

                config.Token = Token;
                config.EmployeeId = EmployeeId;
                config.DisplayName = DisplayName;
                config.Role = Role;
                Config.Save(config);
            }
            else
            {
                Token = config.Token;
                EmployeeId = config.EmployeeId;
                DisplayName = config.DisplayName;
                Role = config.Role;
            }

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnLastWindowClose; // Restore default behavior for MainWindow
            mainWindow.Show();
        }
    }
}
