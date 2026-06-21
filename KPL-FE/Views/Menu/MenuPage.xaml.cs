using KPL_FE.Controllers;
using KPL_FE.Helpers;
using KPL_FE.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class MenuPage : Page
{
    private readonly MenuApiController _api = new();
    private List<MenuDto> _allMenus = [];
    private bool _isLoading;
    private string? _loadErrorMessage;
    private string? _operationErrorMessage;
    private MenuDto? _pendingDeleteMenu;

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
            _loadErrorMessage = ErrorHelper.GetFriendlyErrorMessage(ex, "menu");
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
        if (App.Role != "Admin")
        {
            MessageDialog.Show("Akses Ditolak", "Hanya admin yang dapat menambah item menu.", MessageDialogButton.OK);
            return;
        }

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

        await DeleteMenuAsync(menu, confirm: true);
    }

    private async Task DeleteMenuAsync(MenuDto menu, bool confirm)
    {
        if (confirm)
        {
            var result = MessageDialog.Show(
                "Hapus Menu",
                $"Hapus menu \"{menu.Name}\"?",
                MessageDialogButton.YesNo);

            if (result != MessageDialogResult.Yes) return;
        }

        _pendingDeleteMenu = menu;
        _operationErrorMessage = null;
        UpdateOperationError();

        try
        {
            _isLoading = true;
            UpdateState();
            await _api.DeleteAsync(menu.Id);
            _pendingDeleteMenu = null;
            await LoadMenus();
        }
        catch (Exception ex)
        {
            _operationErrorMessage = $"Gagal menghapus menu: {ErrorHelper.GetFriendlyErrorMessage(ex, "operasi")}";
            UpdateOperationError();
        }
        finally
        {
            _isLoading = false;
            UpdateState();
        }
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e) => await LoadMenus();

    private async void RetryDeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingDeleteMenu == null) return;
        await DeleteMenuAsync(_pendingDeleteMenu, confirm: false);
    }

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

    private void UpdateOperationError()
    {
        var hasError = !string.IsNullOrWhiteSpace(_operationErrorMessage);
        OperationErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
        {
            OperationErrorText.Text = _operationErrorMessage;
        }
    }
}
