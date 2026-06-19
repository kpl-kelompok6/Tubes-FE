using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class MenuPage : Page
{
    private readonly MenuApiController _api = new();
    private List<MenuDto> _allMenus = [];
    private bool _isLoading;
    private string? _loadErrorMessage;

    public MenuPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadMenus();
    }

    private async Task LoadMenus()
    {
        _isLoading = true;
        _loadErrorMessage = null;
        UpdateState();

        try
        {
            _allMenus = await _api.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _loadErrorMessage = GetFriendlyErrorMessage(ex, "menu");
            UpdateState();
        }
        finally
        {
            _isLoading = false;
            UpdateState();
        }
    }

    private void Filter_Checked(object sender, RoutedEventArgs e) => ApplyFilter();

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        if (MenuItemsControl == null)
            return;

        var filter = FilterAll.IsChecked == true ? null
                    : FilterMakanan.IsChecked == true ? "Makanan"
                    : "Minuman";

        var searchText = SearchTextBox.Text.Trim();

        var filtered = _allMenus
            .Where(m => filter is null || m.Category == filter)
            .Where(m => string.IsNullOrWhiteSpace(searchText) || m.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        MenuItemsControl.ItemsSource = filtered;
        UpdateState(filtered.Count, filter, searchText);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new MenuDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await LoadMenus();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not MenuDto menu) return;

        var dialog = new MenuDialog(menu) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await LoadMenus();
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
            _isLoading = true;
            UpdateState();
            await _api.DeleteAsync(menu.Id);
            await LoadMenus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menghapus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isLoading = false;
            UpdateState();
        }
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e) => await LoadMenus();

    private void UpdateState(int? visibleCount = null, string? filter = null, string? searchText = null)
    {
        var count = visibleCount ?? _allMenus.Count;
        var hasError = !string.IsNullOrWhiteSpace(_loadErrorMessage);
        var hasEmpty = !_isLoading && !hasError && count == 0;

        LoadingOverlay.Visibility = _isLoading ? Visibility.Visible : Visibility.Collapsed;
        ErrorStatePanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        EmptyStatePanel.Visibility = hasEmpty ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
        {
            ErrorMessageText.Text = _loadErrorMessage;
            return;
        }

        if (hasEmpty)
        {
            var filtered = (filter is not null || !string.IsNullOrWhiteSpace(searchText)) && _allMenus.Count > 0;
            EmptyStateTitleText.Text = filtered ? "Tidak ada hasil" : "Data Kosong";
            EmptyStateMessageText.Text = filtered
                ? "Tidak ada menu yang cocok dengan pencarian atau kategori saat ini."
                : "Belum ada menu tersedia.";
        }
    }

    private static string GetFriendlyErrorMessage(Exception ex, string context)
    {
        if (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
            return $"Server tidak merespons saat memuat {context}. Coba Lagi.";

        if (ex is HttpRequestException)
            return $"Tidak dapat terhubung ke server saat memuat {context}. Coba Lagi.";

        return $"Gagal memuat {context}: {ex.Message}";
    }
}
