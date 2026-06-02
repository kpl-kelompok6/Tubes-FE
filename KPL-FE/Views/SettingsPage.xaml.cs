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

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Reset backend URL? This will show the setup dialog on next launch.",
            "Reset Setup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        App.Config.Delete();
        App.BaseUrl = "http://localhost:5146";
        UrlDisplay.Text = App.BaseUrl;

        MessageBox.Show(
            "Reset complete. The setup dialog will appear on next launch.",
            "Reset Setup",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
