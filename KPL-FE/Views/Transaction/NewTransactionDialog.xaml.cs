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
        SetCreating(true);

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
<<<<<<< HEAD
        CreatingRing.IsActive = isCreating;
=======
        CreatingProgressRing.IsActive = isCreating;
>>>>>>> origin/staging
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
