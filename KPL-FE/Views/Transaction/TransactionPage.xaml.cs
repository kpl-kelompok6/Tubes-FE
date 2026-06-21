using KPL_FE.Controllers;
using KPL_FE.Helpers;
using KPL_FE.Models;
using KPL_FE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class TransactionPage : Page
{
    internal static bool PendingNewTransaction;
    internal static int? PendingEditTransactionId;

    private readonly TransactionApiController _txApi = new();
    private readonly MenuApiController _menuApi = new();

    private List<TransactionDto> _transactions = [];
    private List<MenuDto> _allMenus = [];
    private TransactionDto? _selectedTransaction;
    private bool _isRefreshing;
    private bool _isLoadingTransactions;
    private bool _isLoadingMenus;
    private string? _transactionsLoadError;
    private string? _menusLoadError;

    public TransactionPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await LoadTransactionsAsync();
            await LoadMenusAsync();
            if (PendingNewTransaction)
            {
                PendingNewTransaction = false;
                OpenNewTransactionDialog();
            }

            if (PendingEditTransactionId.HasValue)
            {
                var id = PendingEditTransactionId.Value;
                PendingEditTransactionId = null;
                var match = _transactions.Find(t => t.Id == id);
                if (match != null)
                    TransactionsListBox.SelectedItem = match;
            }
        };
    }

    public void TriggerNewTransaction()
    {
        OpenNewTransactionDialog();
    }

    private async void OpenNewTransactionDialog()
    {
        var dialog = new NewTransactionDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.CreatedTransaction != null)
        {
            try
            {
                _isRefreshing = true;
                await LoadTransactionsAsync();
            }
            finally
            {
                _isRefreshing = false;
            }

            // By doing this after _isRefreshing is false, the SelectionChanged
            // event will trigger and fetch the transaction details properly.
            var match = _transactions.Find(t => t.Id == dialog.CreatedTransaction.Id);
            if (match != null)
            {
                TransactionsListBox.SelectedItem = match;
            }
        }
    }

    // ──────────────────────────────────────────────
    // Data Loading
    // ──────────────────────────────────────────────

    private async Task LoadTransactionsAsync()
    {
        _isLoadingTransactions = true;
        _transactionsLoadError = null;
        UpdateTransactionsState();

        try
        {
            var all = await _txApi.GetAllAsync();
            _transactions = all.FindAll(t => t.Status == "Created");
            RefreshTransactionList();
        }
        catch (Exception ex)
        {
            _transactionsLoadError = ErrorHelper.GetFriendlyErrorMessage(ex, "transaksi");
            UpdateTransactionsState();
        }
        finally
        {
            _isLoadingTransactions = false;
            UpdateTransactionsState();
        }
    }

    private async Task LoadMenusAsync()
    {
        _isLoadingMenus = true;
        _menusLoadError = null;
        UpdateMenuState();

        try
        {
            _allMenus = await _menuApi.GetAllAsync();
            // Only show available menus
            _allMenus = _allMenus.Where(m => m.IsAvailable).ToList();
            ApplyMenuFilter();
        }
        catch (Exception ex)
        {
            _menusLoadError = ErrorHelper.GetFriendlyErrorMessage(ex, "menu");
            UpdateMenuState();
        }
        finally
        {
            _isLoadingMenus = false;
            UpdateMenuState();
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
            await DialogService.ShowError("Error", $"Gagal memuat detail transaksi: {ex.Message}");
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
        UpdateTransactionsState();
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
        CancelTransactionButton.IsEnabled = true;
    }

    private void UpdateTransactionsState()
    {
        var hasError = !string.IsNullOrWhiteSpace(_transactionsLoadError);
        var isEmpty = !_isLoadingTransactions && !hasError && _transactions.Count == 0;

        TransactionsLoadingPanel.Visibility = _isLoadingTransactions ? Visibility.Visible : Visibility.Collapsed;
        TransactionsErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        TransactionsEmptyPanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
        {
            TransactionsErrorText.Text = _transactionsLoadError;
        }
    }

    private void UpdateMenuState()
    {
        if (_selectedTransaction == null)
        {
            EmptyStatePanel.Visibility = Visibility.Visible;
            MenuBrowserPanel.Visibility = Visibility.Collapsed;
            MenuLoadingPanel.Visibility = Visibility.Collapsed;
            MenuErrorPanel.Visibility = Visibility.Collapsed;
            MenuEmptyPanel.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyStatePanel.Visibility = Visibility.Collapsed;
        MenuBrowserPanel.Visibility = Visibility.Visible;

        var hasError = !string.IsNullOrWhiteSpace(_menusLoadError);
        var currentFilter = FilterAll.IsChecked == true ? null
                          : FilterMakanan.IsChecked == true ? "Makanan"
                          : "Minuman";
        var filteredCount = _allMenus.Count == 0
            ? 0
            : (currentFilter is null ? _allMenus.Count : _allMenus.Count(m => m.Category == currentFilter));
        var isEmpty = !_isLoadingMenus && !hasError && filteredCount == 0;

        MenuLoadingPanel.Visibility = _isLoadingMenus ? Visibility.Visible : Visibility.Collapsed;
        MenuErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        MenuEmptyPanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
        {
            MenuErrorText.Text = _menusLoadError;
            return;
        }

        if (isEmpty)
        {
            var noMatch = _allMenus.Count > 0;
            MenuEmptyTitleText.Text = noMatch ? "Tidak ada hasil" : "Data Kosong";
            MenuEmptyMessageText.Text = noMatch
                ? "Tidak ada menu yang cocok dengan filter saat ini."
                : "Belum ada menu tersedia.";
        }
    }

    // ──────────────────────────────────────────────
    // Transaction List Events
    // ──────────────────────────────────────────────

    private void NewTransactionButton_Click(object sender, RoutedEventArgs e)
    {
        OpenNewTransactionDialog();
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

        string? filter = FilterAll.IsChecked == true ? null
                        : FilterMakanan.IsChecked == true ? "Makanan"
                        : "Minuman";

        var filtered = filter is null
            ? _allMenus
            : _allMenus.Where(m => m.Category == filter).ToList();

        MenuItemsControl.ItemsSource = filtered;
        UpdateMenuState();
    }

    private async void AddMenuItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null)
        {
            ToastService.ShowInfo("Pilih transaksi terlebih dahulu.");
            return;
        }

        if (sender is not FrameworkElement { Tag: MenuDto menu }) return;

        // Disable button to prevent double-click
        var button = sender as Button;
        if (button != null) button.IsEnabled = false;

        var txId = _selectedTransaction.Id;
        var request = new AddItemRequest
        {
            MenuId = menu.Id,
            Quantity = 1
        };

        try
        {
            await _txApi.AddItemAsync(_selectedTransaction.Id, request);
            await RefreshSelectedTransactionAsync();
        }
        catch (Exception ex)
        {
            await DialogService.ShowError("Error", $"Gagal menambahkan item: {ex.Message}");
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
            await DialogService.ShowError("Error", $"Gagal mengubah jumlah: {ex.Message}");
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
            await DialogService.ShowError("Error", $"Gagal mengubah jumlah: {ex.Message}");
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

        var confirmed = await DialogService.ShowConfirm("Hapus Item", $"Hapus \"{item.MenuName}\" dari keranjang?");
        if (!confirmed) return;

        try
        {
            await _txApi.RemoveItemAsync(_selectedTransaction.Id, item.Id);
            await RefreshSelectedTransactionAsync();
        }
        catch (Exception ex)
        {
            await DialogService.ShowError("Error", $"Gagal menghapus item: {ex.Message}");
        }
    }

    private void SetOperationLoading(bool isLoading)
    {
        OperationLoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        OperationLoadingRing.IsActive = isLoading;
    }

    private void UpdateOperationError()
    {
        var hasError = !string.IsNullOrWhiteSpace(null);
        OperationErrorOverlay.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
        {
            OperationErrorText.Text = null;
        }
    }

    private void DismissOperationErrorButton_Click(object sender, RoutedEventArgs e)
    {
        OperationErrorOverlay.Visibility = Visibility.Collapsed;
    }

    private async void RetryOperationButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadTransactionsAsync();
        await LoadMenusAsync();
    }

    private async void CancelTransactionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;

        var confirmed = await DialogService.ShowConfirm(
            "Batalkan Transaksi",
            $"Yakin ingin membatalkan transaksi {_selectedTransaction.TransactionCode}? Semua item akan dihapus dan aksi ini tidak dapat dibatalkan.");
        if (!confirmed) return;

        try
        {
            await _txApi.CancelAsync(_selectedTransaction.Id);
            _selectedTransaction = null;
            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            await DialogService.ShowError("Error", $"Gagal membatalkan transaksi: {ex.Message}");
        }
    }

    private void PayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;

        PaymentPage.PendingPaymentTransactionId = _selectedTransaction.Id;
        NavigationRootPage.SwitchTo(typeof(PaymentPage));
    }

    private async void RetryTransactionsButton_Click(object sender, RoutedEventArgs e) => await LoadTransactionsAsync();

    private async void RetryMenusButton_Click(object sender, RoutedEventArgs e) => await LoadMenusAsync();
}
