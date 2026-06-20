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
        var result = MessageDialog.Show(
            "Atur Ulang",
            "Atur ulang URL backend? Dialog pengaturan akan muncul pada peluncuran berikutnya.",
            MessageDialogButton.YesNo);

        if (result != MessageDialogResult.Yes) return;

        App.Config.Delete();
        App.BaseUrl = string.Empty;
        UrlDisplay.Text = App.BaseUrl;

        ToastNotificationService.Instance.ShowInfo("Atur ulang selesai. Dialog pengaturan akan muncul pada peluncuran berikutnya.");
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageDialog.Show(
            "Keluar",
            "Keluar dan kembali ke layar login?",
            MessageDialogButton.YesNo);

        if (result != MessageDialogResult.Yes) return;

        var config = App.Config.Load();
        config.Token = null;
        App.Config.Save(config);

        App.Token = null;
        App.EmployeeId = 0;
        App.DisplayName = null;
        App.Role = null;

        var mainWindow = Window.GetWindow(this);
        mainWindow.Hide();

        var login = new LoginPage();
        if (login.ShowDialog() == true)
        {
            config.Token = App.Token;
            config.EmployeeId = App.EmployeeId;
            config.DisplayName = App.DisplayName;
            config.Role = App.Role;
            App.Config.Save(config);

            var newMain = new MainWindow();
            Application.Current.MainWindow = newMain;
            newMain.Show();
            mainWindow.Close();
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
