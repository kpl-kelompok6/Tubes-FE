using KPL_FE.Models;
using System.Linq;
using System.Windows;

namespace KPL_FE.Views;

public partial class TransactionDetailDialog : Window
{
    public TransactionDetailDialog(TransactionHistoryDto history)
    {
        InitializeComponent();

        TxCodeText.Text = $"#{history.TransactionCode}";
        TxDateText.Text = $"{history.DateFormatted} {history.TimeFormatted}";
        TxCustomerText.Text = history.CustomerName ?? "-";
        TxTableText.Text = history.TableNumber ?? "-";
        TxPaymentText.Text = history.PaymentMethodDisplay;
        TxTotalText.Text = history.TotalAmountFormatted;
        TxPaidText.Text = history.PaidAmountFormatted;
        TxChangeText.Text = history.ChangeFormatted;

        var items = history.Items ?? [];
        ItemsControl.ItemsSource = items;
        EmptyItemsText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
