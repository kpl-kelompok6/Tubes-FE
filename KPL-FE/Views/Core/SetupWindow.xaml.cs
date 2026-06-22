using KPL_FE.Controllers;
using System.Windows;
using System.Windows.Media;

namespace KPL_FE.Views;

public partial class SetupWindow : Window
{
    public bool Saved { get; private set; }

    public string BaseUrl
    {
        get => UrlTextBox.Text.Trim();
        set => UrlTextBox.Text = value;
    }

    public SetupWindow()
    {
        InitializeComponent();
        UrlTextBox.TextChanged += (_, _) =>
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(UrlTextBox.Text);
            StatusText.Text = "";
        };
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;
        CancelButton.IsEnabled = false;
        UrlTextBox.IsEnabled = false;
        VerifyLoadingRing.Visibility = Visibility.Visible;
        VerifyLoadingRing.IsActive = true;
        StatusText.Foreground = Brushes.Gray;
        StatusText.Text = "Verifying...";

        var url = BaseUrl;
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "http://" + url;

        using var checker = new HealthChecker();
        var (ok, error) = await checker.VerifyAsync(url);

        if (ok)
        {
            UrlTextBox.Text = url;
            Saved = true;
            DialogResult = true;
            Close();
            return;
        }

        StatusText.Foreground = Brushes.Red;
        StatusText.Text = error;
        SaveButton.IsEnabled = true;
        CancelButton.IsEnabled = true;
        UrlTextBox.IsEnabled = true;
        VerifyLoadingRing.Visibility = Visibility.Collapsed;
        VerifyLoadingRing.IsActive = false;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
