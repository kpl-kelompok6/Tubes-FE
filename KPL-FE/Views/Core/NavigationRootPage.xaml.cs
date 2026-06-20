using KPL_FE.Controllers;
using KPL_FE.Models;
using KPL_FE.ViewControllers;
using KPL_FE.Views;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace KPL_FE.Views;

public partial class NavigationRootPage : UserControl
{
    private static Frame? _rootFrame;
    private readonly NavigationRootViewController _vc;
    private readonly KeyboardShortcutController _shortcuts;

    public static void SwitchTo(Type pageType) => (_rootFrame as ModernWpf.Controls.Frame)?.Navigate(pageType);

    public NavigationRootPage()
    {
        InitializeComponent();
        _rootFrame = RootFrame;

        _vc = new NavigationRootViewController(
            pages: new[]
            {
                new PageItem("Menu", typeof(MenuPage)),
                new PageItem("Transaction", typeof(TransactionPage)),
                new PageItem("Payment", typeof(PaymentPage)),
                new PageItem("History", typeof(HistoryPage)),
                new PageItem("Settings", typeof(SettingsPage)),
            });

        _shortcuts = new KeyboardShortcutController(RootFrame, () => _vc.SelectedPageType);

        PagesList.ItemsSource = _vc.Pages;
        PagesList.SelectedIndex = 0;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Handled) return;

        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.N)
        {
            e.Handled = true;
            _shortcuts.HandleNewTransaction();
            return;
        }

        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            e.Handled = true;
            _shortcuts.HandleFocusSearch();
            return;
        }

        if (e.Key == Key.F5)
        {
            e.Handled = true;
            _shortcuts.HandleRefresh();
            return;
        }
    }

    private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var t = _vc.OnPagesListSelectionChanged(PagesList.SelectedValue, out var shouldNavigate);
        if (!shouldNavigate || t is null) return;

        RootFrame.Navigate(t);
    }

    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        _vc.OnRootFrameNavigated(e.Content);
        PagesList.SelectedValue = _vc.SelectedPageType;
        _vc.ReleaseIgnore();
    }
}
