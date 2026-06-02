using System.Windows;

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
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(UrlTextBox.Text);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Saved = true;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
