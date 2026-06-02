using KPL_FE.Views;
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

    private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var config = App.Config.Load();
        var setup = new SetupWindow { BaseUrl = config.BaseUrl };
        setup.Owner = System.Windows.Window.GetWindow(this);
        setup.ShowDialog();

        if (setup.Saved)
        {
            config.BaseUrl = setup.BaseUrl;
            App.Config.Save(config);
            App.BaseUrl = config.BaseUrl;
            UrlDisplay.Text = config.BaseUrl;
        }
    }
}
