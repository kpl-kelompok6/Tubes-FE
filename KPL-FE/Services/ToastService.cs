using KPL_FE.Views;
using KPL_FE.Views.Controls;

namespace KPL_FE.Services;

public static class ToastService
{
    private static ToastNotification? _notificationControl;

    public static void Register(ToastNotification control)
    {
        _notificationControl = control;
    }

    public static void ShowSuccess(string message)
    {
        Show(message, ToastType.Success);
    }

    public static void ShowError(string message)
    {
        Show(message, ToastType.Error);
    }

    public static void ShowWarning(string message)
    {
        Show(message, ToastType.Warning);
    }

    public static void ShowInfo(string message)
    {
        Show(message, ToastType.Info);
    }

    private static void Show(string message, ToastType type)
    {
        if (_notificationControl == null) return;

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _notificationControl.ShowToast(message, type);
        });
    }
}
