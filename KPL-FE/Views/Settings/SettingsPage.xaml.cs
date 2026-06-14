using KPL_FE.Services;
using KPL_FE.Views;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            UrlDisplay.Text = App.BaseUrl;
            DisplayNameText.Text = App.DisplayName ?? "-";
            RoleText.Text = App.Role ?? "-";

            var config = App.Config.Load();
            ThemeToggle.IsOn = config.AppTheme == "Dark";
        };
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var config = App.Config.Load();
        var setup = new SetupWindow { BaseUrl = config.BaseUrl };
        setup.Owner = Window.GetWindow(this);
        setup.ShowDialog();

        if (!setup.Saved) return;

    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var confirmed = await DialogService.ShowConfirm("Reset Setup", "Reset backend URL? This will show the setup dialog on next launch.");
        if (!confirmed) return;

        App.Config.Delete();
        App.BaseUrl = "http://localhost:5146";
        UrlDisplay.Text = App.BaseUrl;

        await DialogService.ShowInfo("Reset Setup", "Reset complete. The setup dialog will appear on next launch.");
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var confirmed = await DialogService.ShowConfirm("Logout", "Logout and return to login screen?");
        if (!confirmed) return;

        var config = App.Config.Load();
        config.Token = null;
        App.Config.Save(config);

        App.BaseUrl = config.BaseUrl;
        App.Token = null;
        App.EmployeeId = 0;
        App.DisplayName = null;
        App.Role = null;
        UrlDisplay.Text = config.BaseUrl;

        var login = new LoginPage();
        login.ShowDialog();

        if (login.Saved)
        {
            config.Token = App.Token;
            config.EmployeeId = App.EmployeeId;
            config.DisplayName = App.DisplayName;
            config.Role = App.Role;
            App.Config.Save(config);

            DisplayNameText.Text = App.DisplayName ?? "-";
            RoleText.Text = App.Role ?? "-";
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        ThemeManager.Current.ApplicationTheme = ThemeToggle.IsOn ? ApplicationTheme.Dark : ApplicationTheme.Light;

        var config = App.Config.Load();
        config.AppTheme = ThemeToggle.IsOn ? "Dark" : "Light";
        App.Config.Save(config);
    }
}
