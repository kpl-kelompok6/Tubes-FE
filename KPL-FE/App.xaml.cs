using KPL_FE.Controllers;
using KPL_FE.Views;
using System.Windows;

namespace KPL_FE
{
    public partial class App : Application
    {
        public static ConfigController Config { get; } = new();

        public static string BaseUrl { get; internal set; } = "http://localhost:5146";

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (_, args) =>
            {
                MessageBox.Show(args.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            var config = Config.Load();
            var mainWindow = new MainWindow();

            if (!Config.Exists())
            {
                var setup = new SetupWindow { BaseUrl = config.BaseUrl, Owner = mainWindow };
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
            mainWindow.Show();
        }
    }
}
