using System.Windows;
using System.Windows.Input;

namespace KPL_FE.Views;

public partial class ShortcutHelpOverlay : Window
{
    public ShortcutHelpOverlay()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus();
    }

    private void OnOverlayKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
            return;
        }

        if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && e.Key == Key.OemQuestion)
        {
            e.Handled = true;
            Close();
            return;
        }
    }

    private void OnOverlayMouseDown(object sender, MouseButtonEventArgs e)
    {
        Close();
    }
}
