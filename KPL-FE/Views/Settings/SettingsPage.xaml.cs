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

        if (!setup.Saved) return;

        config.BaseUrl = setup.BaseUrl;
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

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Reset backend URL? This will show the setup dialog on next launch.",
            "Reset Setup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        App.Config.Delete();
        App.BaseUrl = string.Empty;
        UrlDisplay.Text = App.BaseUrl;

        MessageBox.Show(
            "Reset complete. The setup dialog will appear on next launch.",
            "Reset Setup",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Logout and return to login screen?",
            "Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

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
