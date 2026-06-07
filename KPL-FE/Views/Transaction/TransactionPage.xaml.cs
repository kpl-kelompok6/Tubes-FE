using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class TransactionPage : Page
{
    private readonly TransactionApiController _txApi = new();
    private readonly MenuApiController _menuApi = new();

    private List<TransactionDto> _transactions = [];
    private List<MenuDto> _allMenus = [];
    private TransactionDto? _selectedTransaction;
    private bool _isRefreshing;

    public TransactionPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await LoadTransactionsAsync();
            await LoadMenusAsync();
        };
    }

    // ──────────────────────────────────────────────
    // Data Loading
    // ──────────────────────────────────────────────

    private async Task LoadTransactionsAsync()
    {
        try
        {
            var all = await _txApi.GetAllAsync();
            _transactions = all.FindAll(t => t.Status == "Created");
            RefreshTransactionList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal memuat transaksi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadMenusAsync()
    {
        try
        {
            _allMenus = await _menuApi.GetAllAsync();
            // Only show available menus
            _allMenus = _allMenus.Where(m => m.IsAvailable).ToList();
            ApplyMenuFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal memuat menu: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshSelectedTransactionAsync()
    {
        if (_selectedTransaction == null) return;

        var txId = _selectedTransaction.Id;

        try
        {
            _isRefreshing = true;

            _selectedTransaction = await _txApi.GetByIdAsync(txId);
            UpdateCartUI();

            // Also refresh the transaction list to update totals
            await LoadTransactionsAsync();

            // Re-select the transaction in the list
            var match = _transactions.Find(t => t.Id == txId);
            if (match != null)
            {
                TransactionsListBox.SelectedItem = match;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal memuat detail transaksi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // ──────────────────────────────────────────────
    // UI Refresh
    // ──────────────────────────────────────────────

    private void RefreshTransactionList()
    {
        TransactionsListBox.ItemsSource = null;
        TransactionsListBox.ItemsSource = _transactions;
    }

    private void RefreshUI()
    {
        if (_selectedTransaction == null)
        {
            EmptyStatePanel.Visibility = Visibility.Visible;
            MenuBrowserPanel.Visibility = Visibility.Collapsed;
            CartPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            MenuBrowserPanel.Visibility = Visibility.Visible;
            CartPanel.Visibility = Visibility.Visible;
            UpdateCartUI();
        }
    }

    private void UpdateCartUI()
    {
        if (_selectedTransaction == null) return;

        SelectedTxCodeText.Text = $"Detail Transaksi {_selectedTransaction.DisplayCode}";
        SelectedTxCustomerText.Text = $"Pelanggan: {_selectedTransaction.CustomerName ?? "-"} (Meja {_selectedTransaction.TableNumber ?? "-"})";
        SelectedTxTotalText.Text = _selectedTransaction.TotalAmountFormatted;

        var items = _selectedTransaction.Items ?? [];
        CartItemsControl.ItemsSource = null;
        CartItemsControl.ItemsSource = items;

        EmptyCartText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        PayButton.IsEnabled = items.Count > 0;
    }

    // ──────────────────────────────────────────────
    // Transaction List Events
    // ──────────────────────────────────────────────

    private async void NewTransactionButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NewTransactionDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.CreatedTransaction != null)
        {
            _isRefreshing = true;
            try
            {
                await LoadTransactionsAsync();

                var newTx = _transactions.Find(t => t.Id == dialog.CreatedTransaction.Id);
                if (newTx != null)
                {
                    TransactionsListBox.SelectedItem = newTx;
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }
    }

    private async void TransactionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Skip if we're in the middle of a programmatic refresh
        if (_isRefreshing) return;

        if (TransactionsListBox.SelectedItem is not TransactionDto selectedTx)
        {
            _selectedTransaction = null;
            RefreshUI();
            return;
        }

        try
        {
            // Fetch full transaction details (with items) from the API
            _selectedTransaction = await _txApi.GetByIdAsync(selectedTx.Id);
        }
        catch
        {
            _selectedTransaction = selectedTx;
        }

        RefreshUI();
    }

    // ──────────────────────────────────────────────
    // Menu Browser Events
    // ──────────────────────────────────────────────

    private void MenuFilter_Checked(object sender, RoutedEventArgs e) => ApplyMenuFilter();

    private void ApplyMenuFilter()
    {
        if (MenuItemsControl == null) return;

        if (_allMenus.Count == 0)
        {
            MenuItemsControl.ItemsSource = null;
            return;
        }

        string? filter = FilterAll.IsChecked == true ? null
                        : FilterMakanan.IsChecked == true ? "Makanan"
                        : "Minuman";

        var filtered = filter is null
            ? _allMenus
            : _allMenus.Where(m => m.Category == filter).ToList();

        MenuItemsControl.ItemsSource = filtered;
    }

    private async void AddMenuItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null)
        {
            MessageBox.Show("Pilih transaksi terlebih dahulu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (sender is not FrameworkElement { Tag: MenuDto menu }) return;

        // Disable button to prevent double-click
        var button = sender as Button;
        if (button != null) button.IsEnabled = false;

        try
        {
            var request = new AddItemRequest
            {
                MenuId = menu.Id,
                Quantity = 1
            };

            await _txApi.AddItemAsync(_selectedTransaction.Id, request);
            await RefreshSelectedTransactionAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menambahkan item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            if (button != null) button.IsEnabled = true;
        }
    }

    // ──────────────────────────────────────────────
    // Cart Events
    // ──────────────────────────────────────────────

    private async void IncreaseQtyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;
        if (sender is not FrameworkElement { Tag: TransactionItemDto item }) return;

        try
        {
            var request = new UpdateItemRequest { Quantity = item.Quantity + 1 };
            await _txApi.UpdateItemQuantityAsync(_selectedTransaction.Id, item.Id, request);
            await RefreshSelectedTransactionAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal mengubah jumlah: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DecreaseQtyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;
        if (sender is not FrameworkElement { Tag: TransactionItemDto item }) return;

        if (item.Quantity <= 1)
        {
            // If quantity would go to 0, remove the item instead
            await RemoveItemAsync(item);
            return;
        }

        try
        {
            var request = new UpdateItemRequest { Quantity = item.Quantity - 1 };
            await _txApi.UpdateItemQuantityAsync(_selectedTransaction.Id, item.Id, request);
            await RefreshSelectedTransactionAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal mengubah jumlah: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;
        if (sender is not FrameworkElement { Tag: TransactionItemDto item }) return;

        await RemoveItemAsync(item);
    }

    private async Task RemoveItemAsync(TransactionItemDto item)
    {
        if (_selectedTransaction == null) return;

        var result = MessageBox.Show(
            $"Hapus \"{item.MenuName}\" dari keranjang?",
            "Hapus Item",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _txApi.RemoveItemAsync(_selectedTransaction.Id, item.Id);
            await RefreshSelectedTransactionAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menghapus item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
