using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public class ToastNotificationService
{
    private static ToastNotificationService? _instance;
    public static ToastNotificationService Instance => _instance ??= new ToastNotificationService();

    private Panel? _container;
    private readonly ObservableCollection<ToastControl> _activeToasts = [];

    public int MaxVisibleToasts { get; set; } = 5;

    private ToastNotificationService() { }

    public void Initialize(Panel container)
    {
        _container = container;
        container.Children.Clear();
    }

    public void ShowSuccess(string message, int durationMs = 4000)
        => ShowToast(ToastType.Success, message, durationMs);

    public void ShowError(string message, int durationMs = 6000)
        => ShowToast(ToastType.Error, message, durationMs);

    public void ShowWarning(string message, int durationMs = 4000)
        => ShowToast(ToastType.Warning, message, durationMs);

    public void ShowInfo(string message, int durationMs = 4000)
        => ShowToast(ToastType.Info, message, durationMs);

    private void ShowToast(ToastType type, string message, int durationMs)
    {
        if (_container == null) return;

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => ShowToast(type, message, durationMs));
            return;
        }

        var toast = ToastControl.Create(type, message, durationMs);
        toast.Dismissed += OnToastDismissed;

        _activeToasts.Add(toast);
        _container.Children.Add(toast);

        while (_activeToasts.Count > MaxVisibleToasts)
        {
            var oldest = _activeToasts[0];
            oldest.Dismiss();
        }

        toast.Show();
    }

    private void OnToastDismissed(object? sender, EventArgs e)
    {
        if (sender is not ToastControl toast) return;

        toast.Dismissed -= OnToastDismissed;
        _activeToasts.Remove(toast);
        _container?.Children.Remove(toast);
    }
}
