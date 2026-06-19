using KPL_FE.Controllers;
using KPL_FE.Models;
using System.Windows;
using System.Windows.Media;

namespace KPL_FE.Views;

public partial class MenuDialog : Window
{
    public bool Saved { get; private set; }

    public int? MenuId { get; }
    public string MenuName
    {
        get => NameBox.Text.Trim();
        set => NameBox.Text = value;
    }
    public string? MenuDescription
    {
        get => string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim();
        set => DescBox.Text = value ?? "";
    }
    public decimal MenuPrice
    {
        get => decimal.TryParse(PriceBox.Text, out var p) ? p : 0;
        set => PriceBox.Text = value.ToString("F0");
    }
    public string MenuCategory
    {
        get => (CategoryCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Makanan";
        set
        {
            foreach (System.Windows.Controls.ComboBoxItem item in CategoryCombo.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    CategoryCombo.SelectedItem = item;
                    return;
                }
            }
        }
    }
    public bool MenuIsAvailable
    {
        get => AvailableCheck.IsChecked == true;
        set => AvailableCheck.IsChecked = value;
    }
    public string? MenuImageUrl
    {
        get => string.IsNullOrWhiteSpace(ImageUrlBox.Text) ? null : ImageUrlBox.Text.Trim();
        set => ImageUrlBox.Text = value ?? "";
    }

    public MenuDialog()
    {
        InitializeComponent();
        NameBox.TextChanged += (_, _) => UpdateSaveButton();
        PriceBox.TextChanged += (_, _) => UpdateSaveButton();
    }

    public MenuDialog(MenuDto menu) : this()
    {
        Title = "Edit Menu";
        TitleText.Text = "Edit Menu";
        MenuId = menu.Id;
        MenuName = menu.Name;
        MenuDescription = menu.Description;
        MenuPrice = menu.Price;
        MenuCategory = menu.Category;
        MenuIsAvailable = menu.IsAvailable;
        MenuImageUrl = menu.ImageUrl;
    }

    private void UpdateSaveButton()
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(NameBox.Text)
            && decimal.TryParse(PriceBox.Text, out var p) && p > 0;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SetSaving(true);

        if (!decimal.TryParse(PriceBox.Text, out var price) || price <= 0)
        {
            ShowError("Harga harus berupa angka positif.");
            SetSaving(false);
            return;
        }

        var request = new MenuRequest
        {
            Name = MenuName,
            Description = MenuDescription,
            Price = price,
            Category = MenuCategory,
            IsAvailable = MenuIsAvailable,
            ImageUrl = MenuImageUrl,
        };

        try
        {
            var api = new MenuApiController();

            if (MenuId.HasValue)
                await api.UpdateAsync(MenuId.Value, request);
            else
                await api.AddAsync(request);

            Saved = true;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError($"Gagal menyimpan: {ex.Message}");
            SetSaving(false);
        }
    }

    private void SetSaving(bool isSaving)
    {
        SaveButton.IsEnabled = !isSaving;
        CancelButton.IsEnabled = !isSaving;
        NameBox.IsEnabled = !isSaving;
        DescBox.IsEnabled = !isSaving;
        PriceBox.IsEnabled = !isSaving;
        CategoryCombo.IsEnabled = !isSaving;
        AvailableCheck.IsEnabled = !isSaving;
        ImageUrlBox.IsEnabled = !isSaving;
        SavingPanel.Visibility = isSaving ? Visibility.Visible : Visibility.Collapsed;
        if (SavingPanel.Children[0] is ModernWpf.Controls.ProgressRing ring)
            ring.IsActive = isSaving;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ShowError(string msg)
    {
        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
