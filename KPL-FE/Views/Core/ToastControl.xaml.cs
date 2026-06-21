using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KPL_FE.Views;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public partial class ToastControl : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(ToastControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(nameof(AccentColor), typeof(Brush), typeof(ToastControl), new PropertyMetadata(null));

    public static readonly DependencyProperty ToastIconProperty =
        DependencyProperty.Register(nameof(ToastIcon), typeof(string), typeof(ToastControl), new PropertyMetadata(string.Empty));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public Brush AccentColor
    {
        get => (Brush)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public string ToastIcon
    {
        get => (string)GetValue(ToastIconProperty);
        set => SetValue(ToastIconProperty, value);
    }

    public ToastType Type { get; private set; }
    public event EventHandler? Dismissed;

    private DispatcherTimer? _timer;
    private bool _isDismissing;

    public ToastControl()
    {
        InitializeComponent();
    }

    public static ToastControl Create(ToastType type, string message, int durationMs = 4000)
    {
        var toast = new ToastControl
        {
            Message = message,
            Type = type
        };

        switch (type)
        {
            case ToastType.Success:
                toast.AccentColor = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                toast.ToastIcon = "\uE930";
                break;
            case ToastType.Error:
                toast.AccentColor = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                toast.ToastIcon = "\uE783";
                break;
            case ToastType.Warning:
                toast.AccentColor = new SolidColorBrush(Color.FromRgb(245, 158, 11));
                toast.ToastIcon = "\uE7BA";
                break;
            case ToastType.Info:
                toast.AccentColor = new SolidColorBrush(Color.FromRgb(59, 130, 246));
                toast.ToastIcon = "\uE946";
                break;
        }

        if (durationMs > 0)
        {
            toast._timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMs)
            };
            toast._timer.Tick += (_, _) => toast.Dismiss();
            toast._timer.Start();
        }

        return toast;
    }

    public async void Show()
    {
        Opacity = 0;
        var translate = new TranslateTransform { X = 100 };
        RenderTransform = translate;

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
        var slideIn = new DoubleAnimation(100, 0, TimeSpan.FromMilliseconds(300));

        BeginAnimation(OpacityProperty, fadeIn);
        translate.BeginAnimation(TranslateTransform.XProperty, slideIn);

        await Task.Delay(300);
    }

    public async void Dismiss()
    {
        if (_isDismissing) return;
        _isDismissing = true;

        _timer?.Stop();
        _timer = null;

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
        var slideOut = new DoubleAnimation(0, 100, TimeSpan.FromMilliseconds(250));
        var translate = RenderTransform as TranslateTransform ?? new TranslateTransform();
        RenderTransform = translate;

        fadeOut.Completed += (_, _) =>
        {
            Dismissed?.Invoke(this, EventArgs.Empty);
        };

        BeginAnimation(OpacityProperty, fadeOut);
        translate.BeginAnimation(TranslateTransform.XProperty, slideOut);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Dismiss();
    }
}
