using KPL_FE.Controllers;
using KPL_FE.Services;
using KPL_FE.Views;
using System.Net.Http;
using System.Windows;

namespace KPL_FE
{
    public partial class App : Application
    {
        public static ConfigController Config { get; } = new();

        public static string BaseUrl { get; internal set; } = "http://localhost:5146";

        public static string? Token { get; set; }
        public static int EmployeeId { get; set; }
        public static string? DisplayName { get; set; }
        public static string? Role { get; set; }

        public static HttpClient ApiHttp { get; } = new(new AuthHandler())
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

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

            if (!Config.Exists())
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
