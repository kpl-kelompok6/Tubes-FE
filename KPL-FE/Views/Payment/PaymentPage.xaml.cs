using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public partial class PaymentPage : Page
{
    private readonly PaymentApiController _paymentApi = new();
    private readonly HistoryApiController _historyApi = new();

    private List<PaymentResponse> _payments = [];
    private bool _isLoading;
    private string? _loadError;

    public PaymentPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadPaymentsAsync();
    }

    private async Task LoadPaymentsAsync()
    {
        _isLoading = true;
        _loadError = null;
        UpdateState();

        try
        {
            _payments = await _paymentApi.GetAllPaymentsAsync();
            RefreshList();
        }
        catch (Exception ex)
        {
            _loadError = GetFriendlyErrorMessage(ex, "pembayaran");
            UpdateState();
        }
        finally
        {
            _isLoading = false;
            UpdateState();
        }
    }

    private void RefreshList()
    {
        PaymentsListBox.ItemsSource = null;
        PaymentsListBox.ItemsSource = _payments;
        UpdateState();
    }

    private void UpdateState()
    {
        var hasError = !string.IsNullOrWhiteSpace(_loadError);

        LoadingPanel.Visibility = _isLoading ? Visibility.Visible : Visibility.Collapsed;
        ErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        EmptyPanel.Visibility = !_isLoading && !hasError && _payments.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (hasError)
            ErrorText.Text = _loadError;
    }

    private async void PaymentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PaymentsListBox.SelectedItem is not PaymentResponse selected)
        {
            PaymentsListBox.SelectedItem = null;
            return;
        }

        try
        {
            var detail = await _historyApi.GetByIdAsync(selected.TransactionId);
            var dialog = new TransactionDetailDialog(detail)
            {
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }
        catch
        {
            MessageBox.Show("Gagal memuat detail pembayaran.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        PaymentsListBox.SelectedItem = null;
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e) => await LoadPaymentsAsync();

    private static string GetFriendlyErrorMessage(Exception ex, string context)
    {
        if (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
            return $"Server tidak merespons saat memuat {context}. Coba Lagi.";

        if (ex is HttpRequestException)
            return $"Tidak dapat terhubung ke server saat memuat {context}. Coba Lagi.";

        return $"Gagal memuat {context}: {ex.Message}";
    }
}