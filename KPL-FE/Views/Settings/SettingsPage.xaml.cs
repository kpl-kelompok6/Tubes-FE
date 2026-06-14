using KPL_FE.Services;
using KPL_FE.Views;
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
        };
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        var config = App.Config.Load();
        var setup = new SetupWindow { BaseUrl = config.BaseUrl };
        setup.Owner = Window.GetWindow(this);
        setup.ShowDialog();

        if (setup.Saved)
        {
            config.BaseUrl = setup.BaseUrl;
            App.Config.Save(config);
            App.BaseUrl = config.BaseUrl;
            UrlDisplay.Text = config.BaseUrl;
        }
    }

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

        App.Token = null;
        App.EmployeeId = 0;
        App.DisplayName = null;
        App.Role = null;

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
            System.Windows.Application.Current.Shutdown();
        }
    }
}
