using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Windows;

namespace KPL_FE.Views;

public partial class NewTransactionDialog : Window
{
    private readonly TransactionApiController _api = new();

    public TransactionDto? CreatedTransaction { get; private set; }

    public NewTransactionDialog()
    {
        InitializeComponent();
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        var tableNumber = string.IsNullOrWhiteSpace(TableNumberBox.Text) ? null : TableNumberBox.Text.Trim();
        
        if (tableNumber != null && !System.Text.RegularExpressions.Regex.IsMatch(tableNumber, "^[0-9]+$"))
        {
            MessageBox.Show("Nomor meja hanya boleh berisi angka.", "Validasi Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetCreating(true);

        var request = new CreateTransactionRequest
        {
            CustomerName = string.IsNullOrWhiteSpace(CustomerNameBox.Text) ? null : CustomerNameBox.Text.Trim(),
            TableNumber = tableNumber
        };

        try
        {
            CreatedTransaction = await _api.CreateAsync(request);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal membuat transaksi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SetCreating(false);
        }
    }

    private void SetCreating(bool isCreating)
    {
        CreateButton.IsEnabled = !isCreating;
        CancelButton.IsEnabled = !isCreating;
        CustomerNameBox.IsEnabled = !isCreating;
        TableNumberBox.IsEnabled = !isCreating;
        CreatingPanel.Visibility = isCreating ? Visibility.Visible : Visibility.Collapsed;
        CreatingProgressRing.IsActive = isCreating;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
