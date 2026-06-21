using KPL_FE.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KPL_FE.Views.Controls;

public partial class ToastNotification : UserControl
{
    private const int MaxVisibleToasts = 5;

    private static readonly Dictionary<ToastType, SolidColorBrush> TypeBrushes = new()
    {
        [ToastType.Success] = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
        [ToastType.Error] = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
        [ToastType.Warning] = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
        [ToastType.Info] = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
    };

    private static readonly Dictionary<ToastType, int> TypeDurations = new()
    {
        [ToastType.Success] = 4000,
        [ToastType.Error] = 6000,
        [ToastType.Warning] = 6000,
        [ToastType.Info] = 4000,
    };

    private static readonly Dictionary<ToastType, string> TypeIcons = new()
    {
        [ToastType.Success] = "\u2713",
        [ToastType.Error] = "\u2717",
        [ToastType.Warning] = "\u26A0",
        [ToastType.Info] = "\u2139",
    };

    private readonly List<Border> _activeToasts = [];

    public ToastNotification()
    {
        InitializeComponent();
        ToastService.Register(this);
    }

    public void ShowToast(string message, ToastType type)
    {
        var brush = TypeBrushes[type];
        var duration = TypeDurations[type];
        var icon = TypeIcons[type];

        var toast = CreateToast(icon, message, brush);
        _activeToasts.Add(toast);
        ToastContainer.Children.Add(toast);

        if (_activeToasts.Count > MaxVisibleToasts)
        {
            RemoveToast(_activeToasts[0], instantly: true);
        }

        toast.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)));

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(duration)
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            RemoveToast(toast, instantly: false);
        };
        timer.Start();
    }

    private static Border CreateToast(string icon, string message, SolidColorBrush accentBrush)
    {
        var border = new Border
        {
            Width = 360,
            Margin = new Thickness(0, 0, 0, 8),
            Background = FindResourceOrDefault("SystemControlBackgroundChromeMediumLowBrush", new SolidColorBrush(Color.FromRgb(30, 30, 30))),
            Opacity = 0,
        };

        var innerGrid = new Grid();
        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var accentBar = new Border
        {
            Background = accentBrush,
            Width = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Grid.SetColumn(accentBar, 0);
        innerGrid.Children.Add(accentBar);

        var iconBlock = new TextBlock
        {
            Text = icon,
            FontSize = 18,
            Foreground = accentBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 12, 8, 12),
        };
        Grid.SetColumn(iconBlock, 1);
        innerGrid.Children.Add(iconBlock);

        var msgBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 12, 12, 12),
            Foreground = FindResourceOrDefault("SystemControlForegroundBaseHighBrush", new SolidColorBrush(Colors.White)),
        };
        Grid.SetColumn(msgBlock, 2);
        innerGrid.Children.Add(msgBlock);

        border.Child = innerGrid;
        return border;
    }

    private void RemoveToast(Border toast, bool instantly)
    {
        if (!_activeToasts.Contains(toast)) return;

        if (instantly)
        {
            _activeToasts.Remove(toast);
            ToastContainer.Children.Remove(toast);
            return;
        }

        var anim = new DoubleAnimation(toast.Opacity, 0, TimeSpan.FromMilliseconds(300));
        anim.Completed += (_, _) =>
        {
            _activeToasts.Remove(toast);
            ToastContainer.Children.Remove(toast);
        };
        toast.BeginAnimation(OpacityProperty, anim);
    }

    private static T FindResourceOrDefault<T>(string key, T defaultValue) where T : class
    {
        try
        {
            return Application.Current.TryFindResource(key) as T ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}
