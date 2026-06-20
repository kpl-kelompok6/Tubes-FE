using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KPL_FE.Views.Payment;

public partial class PaymentDialog : Window
{
    private readonly PaymentApiController _paymentApi = new();
    private readonly TransactionDto _transaction;
    
    public PaymentResponse? Result { get; private set; }

    public PaymentDialog(TransactionDto transaction)
    {
        InitializeComponent();
        _transaction = transaction;
        
        TotalTagihanText.Text = _transaction.TotalAmountFormatted;
        PaidAmountTextBox.Focus();
    }

    private void PaidAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        
        if (decimal.TryParse(PaidAmountTextBox.Text, out decimal paidAmount))
        {
            var change = paidAmount - _transaction.TotalAmount;
            if (change >= 0)
            {
                ChangeAmountText.Text = $"Rp {change:N0}";
                ChangeAmountText.Foreground = Brushes.Green;
                SubmitButton.IsEnabled = true;
            }
            else
            {
                ChangeAmountText.Text = $"Kurang Rp {Math.Abs(change):N0}";
                ChangeAmountText.Foreground = Brushes.Red;
                SubmitButton.IsEnabled = false;
            }
        }
        else
        {
            ChangeAmountText.Text = "Rp 0";
            ChangeAmountText.Foreground = Brushes.Gray;
            SubmitButton.IsEnabled = false;
        }
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(PaidAmountTextBox.Text, out decimal paidAmount)) return;

        SetProcessing(true);
        try
        {
            var req = new PaymentRequest
            {
                TransactionId = _transaction.Id,
                PaidAmount = paidAmount,
                PaymentMethod = "cash"
            };

            Result = await _paymentApi.ProcessPaymentAsync(req);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
            ErrorText.Visibility = Visibility.Visible;
            SetProcessing(false);
        }
    }

    private void SetProcessing(bool isProcessing)
    {
        SubmitButton.IsEnabled = !isProcessing;
        CancelButton.IsEnabled = !isProcessing;
        PaidAmountTextBox.IsEnabled = !isProcessing;
        PaymentLoadingPanel.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
        if (PaymentLoadingPanel.Children[0] is ModernWpf.Controls.ProgressRing ring)
            ring.IsActive = isProcessing;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
