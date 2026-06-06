using KPL_FE.Controllers;
using KPL_FE.Models;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class MenuPage : Page
{
    private readonly MenuApiController _api = new();
    private List<MenuDto> _allMenus = [];

    public MenuPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadMenus();
    }

    private async Task LoadMenus()
    {
        try
        {
            _allMenus = await _api.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal memuat menu: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Filter_Checked(object sender, RoutedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        if (_allMenus.Count == 0) return;

        var filter = FilterAll.IsChecked == true ? null
                    : FilterMakanan.IsChecked == true ? "Makanan"
                    : "Minuman";

        MenuItemsControl.ItemsSource = filter is null
            ? _allMenus
            : _allMenus.Where(m => m.Category == filter).ToList();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new MenuDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            _ = LoadMenus();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not MenuDto menu) return;

        var dialog = new MenuDialog(menu) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            _ = LoadMenus();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not MenuDto menu) return;

        var result = MessageBox.Show(
            $"Hapus menu \"{menu.Name}\"?",
            "Hapus Menu",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _api.DeleteAsync(menu.Id);
            _ = LoadMenus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menghapus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
