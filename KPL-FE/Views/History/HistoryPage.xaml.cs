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

public partial class HistoryPage : Page
{
    private readonly HistoryApiController _historyApi = new();

    private List<TransactionHistoryDto> _histories = [];
    private ReportDto? _report;
    private bool _isLoadingHistories;
    private bool _isLoadingReport;
    private string? _historiesLoadError;

    public HistoryPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;
            await LoadDataAsync();
        };
    }

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadHistoriesAsync(),
            LoadReportAsync());
    }

    private async Task LoadHistoriesAsync()
    {
        _isLoadingHistories = true;
        _historiesLoadError = null;
        UpdateHistoryState();

        try
        {
            var start = StartDatePicker.SelectedDate;
            var end = EndDatePicker.SelectedDate;
            _histories = await _historyApi.GetFilteredAsync(start, end);
            RefreshHistoryList();
        }
        catch (Exception ex)
        {
            _historiesLoadError = GetFriendlyErrorMessage(ex, "riwayat");
            UpdateHistoryState();
        }
        finally
        {
            _isLoadingHistories = false;
            UpdateHistoryState();
        }
    }

    private async Task LoadReportAsync()
    {
        _isLoadingReport = true;
        UpdateReportState();

        try
        {
            var start = StartDatePicker.SelectedDate;
            var end = EndDatePicker.SelectedDate;
            _report = await _historyApi.GetReportAsync(start, end);
            UpdateReportUI();
        }
        catch
        {
            _report = null;
            UpdateReportState();
        }
        finally
        {
            _isLoadingReport = false;
            UpdateReportState();
        }
    }

    private void RefreshHistoryList()
    {
        HistoryListBox.ItemsSource = null;
        HistoryListBox.ItemsSource = _histories;
        UpdateHistoryState();
    }

    private void UpdateHistoryState()
    {
        var hasError = !string.IsNullOrWhiteSpace(_historiesLoadError);
        var isEmpty = !_isLoadingHistories && !hasError && _histories.Count == 0;
        var filterApplied = StartDatePicker.SelectedDate.HasValue || EndDatePicker.SelectedDate.HasValue;

        HistoryLoadingPanel.Visibility = _isLoadingHistories ? Visibility.Visible : Visibility.Collapsed;
        HistoryErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        HistoryEmptyPanel.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;

        if (hasError)
        {
            HistoryErrorText.Text = _historiesLoadError;
        }

        if (isEmpty)
        {
            HistoryEmptyText.Text = filterApplied
                ? "Tidak ada transaksi pada rentang tanggal tersebut."
                : "Belum ada riwayat transaksi.";
        }
    }

    private void UpdateReportState()
    {
        ReportLoadingPanel.Visibility = _isLoadingReport ? Visibility.Visible : Visibility.Collapsed;
        ReportEmptyPanel.Visibility = !_isLoadingReport && _report == null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateReportUI()
    {
        if (_report == null) return;

        ReportTotalTxText.Text = _report.TotalTransactions.ToString("N0");
        ReportRevenueText.Text = _report.TotalRevenueFormatted;
        ReportAverageText.Text = _report.AverageTransactionFormatted;

        BreakdownItemsControl.ItemsSource = null;
        BreakdownItemsControl.ItemsSource = _report.Breakdown;

        UpdateReportState();
    }

    private async void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async void ResetFilterButton_Click(object sender, RoutedEventArgs e)
    {
        StartDatePicker.SelectedDate = null;
        EndDatePicker.SelectedDate = null;
        await LoadDataAsync();
    }

    private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HistoryListBox.SelectedItem is not TransactionHistoryDto selected) return;

        var dialog = new TransactionDetailDialog(selected)
        {
            Owner = Window.GetWindow(this)
        };
        dialog.ShowDialog();

        HistoryListBox.SelectedItem = null;
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

    private static string GetFriendlyErrorMessage(Exception ex, string context)
    {
        if (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
            return $"Server tidak merespons saat memuat {context}. Coba Lagi.";

        if (ex is HttpRequestException)
            return $"Tidak dapat terhubung ke server saat memuat {context}. Coba Lagi.";

        return $"Gagal memuat {context}: {ex.Message}";
    }
}
