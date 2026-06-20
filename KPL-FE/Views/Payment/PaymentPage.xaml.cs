using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KPL_FE.Views;

public partial class PaymentPage : Page
{
    internal static int? PendingPaymentTransactionId;

    private readonly TransactionApiController _txApi = new();
    private readonly PaymentApiController _paymentApi = new();

    private List<TransactionDto> _activeTransactions = [];
    private TransactionDto? _selectedTransaction;
    private bool _isLoading;
    private string? _loadError;
    private TransactionDto? _successTransaction;
    private decimal _changeAmount;

    public PaymentPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (PendingPaymentTransactionId.HasValue)
        {
            var id = PendingPaymentTransactionId.Value;
            PendingPaymentTransactionId = null;
            await LoadPaymentFormAsync(id);
        }
        else
        {
            await LoadActiveTransactionsAsync();
        }
    }

    // ──────────────────────────────────────────────
    // Active Transaction List
    // ──────────────────────────────────────────────

    private async Task LoadActiveTransactionsAsync()
    {
        _isLoading = true;
        _loadError = null;
        ShowState(ActiveListPanel);
        UpdateActiveListState();

        try
        {
            var all = await _txApi.GetAllAsync();
            _activeTransactions = all.FindAll(t => t.Status == "Created");
            RefreshActiveList();
        }
        catch (Exception ex)
        {
            _loadError = GetFriendlyErrorMessage(ex, "transaksi");
            UpdateActiveListState();
        }
        finally
        {
            _isLoading = false;
            UpdateActiveListState();
        }
    }

    private void RefreshActiveList()
    {
        ActiveTxListBox.ItemsSource = null;
        ActiveTxListBox.ItemsSource = _activeTransactions;
        UpdateActiveListState();
    }

    private void UpdateActiveListState()
    {
        var hasError = !string.IsNullOrWhiteSpace(_loadError);
        var isEmpty = !_isLoading && !hasError && _activeTransactions.Count == 0;

        LoadingPanel.Visibility = _isLoading ? Visibility.Visible : Visibility.Collapsed;
        ErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        EmptyPanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
            ErrorText.Text = _loadError;
    }

    // ──────────────────────────────────────────────
    // Payment Form
    // ──────────────────────────────────────────────

    private async Task LoadPaymentFormAsync(int transactionId)
    {
        _isLoading = true;
        _loadError = null;
        LoadingPanel.Visibility = Visibility.Visible;

        try
        {
            var tx = await _txApi.GetByIdAsync(transactionId);

            if (tx.Status != "Created")
            {
                _loadError = "Transaksi ini sudah dibayar atau tidak tersedia.";
                _isLoading = false;
                ShowState(ActiveListPanel);
                UpdateActiveListState();
                return;
            }

            _selectedTransaction = tx;
            ShowPaymentForm();
        }
        catch (Exception ex)
        {
            _loadError = GetFriendlyErrorMessage(ex, "transaksi");
            _isLoading = false;
            ShowState(ActiveListPanel);
            UpdateActiveListState();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ShowPaymentForm()
    {
        if (_selectedTransaction == null) return;

        TxCodeText.Text = $"Transaksi {_selectedTransaction.DisplayCode}";
        TxStatusText.Text = _selectedTransaction.Status;
        TxCustomerText.Text = $"Pelanggan: {_selectedTransaction.CustomerName ?? "-"} (Meja {_selectedTransaction.TableNumber ?? "-"})";
        TxTotalText.Text = _selectedTransaction.TotalAmountFormatted;
        TxItemsControl.ItemsSource = _selectedTransaction.Items ?? [];

        PaidAmountTextBox.Text = "";
        ChangeAmountText.Text = "Rp 0";
        ChangeAmountText.Foreground = Brushes.Green;
        PaymentMethodCombo.SelectedIndex = 0;
        ConfirmPayButton.IsEnabled = false;
        PaymentErrorText.Visibility = Visibility.Collapsed;
        PaymentLoadingPanel.Visibility = Visibility.Collapsed;
        FormLoadingOverlay.Visibility = Visibility.Collapsed;

        ShowState(PaymentFormPanel);
    }

    private void PaidAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        PaymentErrorText.Visibility = Visibility.Collapsed;

        if (_selectedTransaction != null && decimal.TryParse(PaidAmountTextBox.Text, out decimal paidAmount))
        {
            var change = paidAmount - _selectedTransaction.TotalAmount;
            if (change >= 0)
            {
                ChangeAmountText.Text = $"Rp {change:N0}";
                ChangeAmountText.Foreground = Brushes.Green;
                ConfirmPayButton.IsEnabled = true;
            }
            else
            {
                ChangeAmountText.Text = $"Kurang Rp {Math.Abs(change):N0}";
                ChangeAmountText.Foreground = Brushes.Red;
                ConfirmPayButton.IsEnabled = false;
            }
        }
        else
        {
            ChangeAmountText.Text = "Rp 0";
            ChangeAmountText.Foreground = Brushes.Gray;
            ConfirmPayButton.IsEnabled = false;
        }
    }

    private async void ConfirmPayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;
        if (!decimal.TryParse(PaidAmountTextBox.Text, out decimal paidAmount)) return;

        SetPaymentProcessing(true);
        try
        {
            var method = (PaymentMethodCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Cash";
            var req = new PaymentRequest
            {
                TransactionId = _selectedTransaction.Id,
                PaidAmount = paidAmount,
                PaymentMethod = method
            };

            var result = await _paymentApi.ProcessPaymentAsync(req);
            _successTransaction = _selectedTransaction;
            _changeAmount = result.ChangeAmount;
            ShowSuccess();
        }
        catch (Exception ex)
        {
            PaymentErrorText.Text = ex.Message;
            PaymentErrorText.Visibility = Visibility.Visible;
            SetPaymentProcessing(false);
        }
    }

    private void SetPaymentProcessing(bool isProcessing)
    {
        ConfirmPayButton.IsEnabled = !isProcessing;
        CancelPayButton.IsEnabled = !isProcessing;
        PaidAmountTextBox.IsEnabled = !isProcessing;
        PaymentMethodCombo.IsEnabled = !isProcessing;
        FormLoadingOverlay.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CancelPayButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedTransaction = null;
        _ = LoadActiveTransactionsAsync();
    }

    // ──────────────────────────────────────────────
    // Success State
    // ──────────────────────────────────────────────

    private void ShowSuccess()
    {
        if (_successTransaction == null) return;

        SuccessChangeText.Text = $"Kembalian: Rp {_changeAmount:N0}";
        ShowState(SuccessPanel);
    }

    private void SuccessDoneButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedTransaction = null;
        _successTransaction = null;
        _ = LoadActiveTransactionsAsync();
    }

    // ──────────────────────────────────────────────
    // List Selection & Error/Retry
    // ──────────────────────────────────────────────

    private async void ActiveTxListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveTxListBox.SelectedItem is not TransactionDto selected) return;
        ActiveTxListBox.SelectedItem = null;
        await LoadPaymentFormAsync(selected.Id);
    }

    private void EditFromListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: int id }) return;
        TransactionPage.PendingEditTransactionId = id;
        NavigationRootPage.SwitchTo(typeof(TransactionPage));
    }

    private void EditFromSummaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransaction == null) return;
        TransactionPage.PendingEditTransactionId = _selectedTransaction.Id;
        NavigationRootPage.SwitchTo(typeof(TransactionPage));
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e) => await LoadActiveTransactionsAsync();

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private void ShowState(FrameworkElement activePanel)
    {
        ActiveListPanel.Visibility = activePanel == ActiveListPanel ? Visibility.Visible : Visibility.Collapsed;
        PaymentFormPanel.Visibility = activePanel == PaymentFormPanel ? Visibility.Visible : Visibility.Collapsed;
        SuccessPanel.Visibility = activePanel == SuccessPanel ? Visibility.Visible : Visibility.Collapsed;
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
