using KPL_FE.Controllers;
using KPL_FE.Helpers;
using KPL_FE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private string? _reportLoadError;

    public HistoryPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var today = DateTime.Today;
            StartDatePicker.DisplayDateEnd = today;
            EndDatePicker.DisplayDateEnd = today;
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = today;
            await LoadDataAsync();
        };
    }

    private async Task LoadDataAsync()
    {
        if (!ValidateDateFilter()) return;

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
            _historiesLoadError = ErrorHelper.GetFriendlyErrorMessage(ex, "riwayat");
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
        _reportLoadError = null;
        UpdateReportState();

        try
        {
            var start = StartDatePicker.SelectedDate;
            var end = EndDatePicker.SelectedDate;
            _report = await _historyApi.GetReportAsync(start, end);
            UpdateReportUI();
        }
        catch (Exception ex)
        {
            _report = null;
            _reportLoadError = ErrorHelper.GetFriendlyErrorMessage(ex, "laporan");
            UpdateReportUI();
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
        var hasError = !string.IsNullOrWhiteSpace(_reportLoadError);
        ReportLoadingPanel.Visibility = _isLoadingReport ? Visibility.Visible : Visibility.Collapsed;
        ReportErrorPanel.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
        ReportEmptyPanel.Visibility = !_isLoadingReport && !hasError && _report == null ? Visibility.Visible : Visibility.Collapsed;
        BreakdownEmptyPanel.Visibility = !_isLoadingReport && !hasError && _report != null && _report.Breakdown.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (hasError)
        {
            ReportErrorText.Text = _reportLoadError;
        }
    }

    private void UpdateReportUI()
    {
        if (_report == null)
        {
            ReportTotalTxText.Text = "0";
            ReportRevenueText.Text = "Rp 0";
            ReportAverageText.Text = "Rp 0";
            BreakdownItemsControl.ItemsSource = null;
            UpdateReportState();
            return;
        }

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
        ClearDateFilterValidation();
        await LoadDataAsync();
    }

    private void DatePicker_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        ClearDateFilterValidation();
    }

    private bool ValidateDateFilter()
    {
        var start = StartDatePicker.SelectedDate?.Date;
        var end = EndDatePicker.SelectedDate?.Date;
        var today = DateTime.Today;

        if (start > today || end > today)
        {
            ShowDateFilterValidation("Tanggal filter tidak boleh melebihi hari ini.");
            return false;
        }

        if (start.HasValue && end.HasValue && start > end)
        {
            ShowDateFilterValidation("Tanggal mulai tidak boleh lebih besar dari tanggal akhir.");
            return false;
        }

        ClearDateFilterValidation();
        return true;
    }

    private void ShowDateFilterValidation(string message)
    {
        DateFilterValidationText.Text = message;
        DateFilterValidationText.Visibility = Visibility.Visible;
        StartDateErrorBorder.Visibility = Visibility.Visible;
        EndDateErrorBorder.Visibility = Visibility.Visible;
    }

    private void ClearDateFilterValidation()
    {
        DateFilterValidationText.Text = string.Empty;
        DateFilterValidationText.Visibility = Visibility.Collapsed;
        StartDateErrorBorder.Visibility = Visibility.Collapsed;
        EndDateErrorBorder.Visibility = Visibility.Collapsed;
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

    private async void RetryReportButton_Click(object sender, RoutedEventArgs e) => await LoadReportAsync();
}
