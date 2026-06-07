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
        CreateButton.IsEnabled = false;
        CancelButton.IsEnabled = false;
        CustomerNameBox.IsEnabled = false;
        TableNumberBox.IsEnabled = false;

        var request = new CreateTransactionRequest
        {
            CustomerName = string.IsNullOrWhiteSpace(CustomerNameBox.Text) ? null : CustomerNameBox.Text.Trim(),
            TableNumber = string.IsNullOrWhiteSpace(TableNumberBox.Text) ? null : TableNumberBox.Text.Trim()
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
            
            CreateButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            CustomerNameBox.IsEnabled = true;
            TableNumberBox.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
