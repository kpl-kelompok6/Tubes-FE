using KPL_FE.Models;
using KPL_FE.ViewControllers;
using KPL_FE.Views;
using ModernWpf.Controls;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace KPL_FE.Views;

public partial class NavigationRootPage : UserControl
{
    private readonly NavigationRootViewController _vc;

    public NavigationRootPage()
    {
        InitializeComponent();

        _vc = new NavigationRootViewController(
            pages: new[]
            {
                new PageItem("Theme Resources", typeof(ThemeResourcesPage)),
                new PageItem("Themes", typeof(ThemesPage)),
            });

        PagesList.ItemsSource = _vc.Pages;
        PagesList.SelectedIndex = 0;
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
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        sender.ItemsSource = _vc.OnSearchTextChanged(sender.Text, args.Reason);
    }

    private void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var t = _vc.OnSearchQuerySubmitted(args.ChosenSuggestion, args.QueryText);
        if (t is null) return;

        RootFrame.Navigate(t);
    }
}
