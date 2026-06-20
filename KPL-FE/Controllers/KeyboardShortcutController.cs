using KPL_FE.Views;
using System;
using System.Windows.Controls;

namespace KPL_FE.Controllers;

public sealed class KeyboardShortcutController
{
    private readonly Frame _frame;
    private readonly Func<Type?> _getCurrentPageType;

    public KeyboardShortcutController(Frame frame, Func<Type?> getCurrentPageType)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        _getCurrentPageType = getCurrentPageType ?? throw new ArgumentNullException(nameof(getCurrentPageType));
    }

    public void HandleNewTransaction()
    {
        var target = typeof(TransactionPage);
        if (_getCurrentPageType() != target)
        {
            Views.TransactionPage.PendingNewTransaction = true;
            _frame.Navigate(target);
        }
        else if (_frame.Content is Views.TransactionPage txPage)
        {
            txPage.TriggerNewTransaction();
        }
    }

    public void HandleFocusSearch()
    {
        var target = typeof(MenuPage);
        if (_getCurrentPageType() != target)
        {
            _frame.Navigate(target);
        }
    }

    public void HandleRefresh()
    {
        var current = _frame.Content as Page;
        if (current == null) return;
        _frame.Navigate(current.GetType());
    }
}
