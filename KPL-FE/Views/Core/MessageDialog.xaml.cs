using System.Windows;
using System.Windows.Controls;

namespace KPL_FE.Views;

public enum MessageDialogButton
{
    OK,
    OKCancel,
    YesNo
}

public enum MessageDialogResult
{
    OK,
    Cancel,
    Yes,
    No
}

public partial class MessageDialog : Window
{
    public MessageDialogResult Result { get; private set; } = MessageDialogResult.Cancel;

    private MessageDialog(string title, string message, MessageDialogButton buttons)
    {
        InitializeComponent();
        Title = title;
        TitleText.Text = title;
        MessageText.Text = message;
        SetupButtons(buttons);
    }

    private void SetupButtons(MessageDialogButton buttons)
    {
        switch (buttons)
        {
            case MessageDialogButton.OK:
                AddButton("OK", MessageDialogResult.OK, isPrimary: true, isDefault: true);
                break;

            case MessageDialogButton.OKCancel:
                AddButton("Batal", MessageDialogResult.Cancel, isPrimary: false, isDefault: false);
                AddButton("OK", MessageDialogResult.OK, isPrimary: true, isDefault: true);
                break;

            case MessageDialogButton.YesNo:
                AddButton("Tidak", MessageDialogResult.No, isPrimary: false, isDefault: false);
                AddButton("Ya", MessageDialogResult.Yes, isPrimary: true, isDefault: true);
                break;
        }
    }

    private void AddButton(string text, MessageDialogResult result, bool isPrimary, bool isDefault)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 100,
            Margin = new Thickness(8, 0, 0, 0),
            IsDefault = isDefault,
            Style = isPrimary
                ? FindResource("AccentButtonStyle") as Style ?? (Style)FindResource(typeof(Button))
                : (Style)FindResource(typeof(Button))
        };

        if (isPrimary)
        {
            button.Style = FindResource("AccentButtonStyle") as Style ?? (Style)FindResource(typeof(Button));
        }

        button.Click += (_, _) =>
        {
            Result = result;
            DialogResult = true;
            Close();
        };

        ButtonPanel.Children.Add(button);
    }

    public static MessageDialogResult Show(string title, string message, MessageDialogButton buttons = MessageDialogButton.OK)
    {
        var dialog = new MessageDialog(title, message, buttons);
        dialog.Owner = GetActiveWindow();
        dialog.ShowDialog();
        return dialog.Result;
    }

    private static Window? GetActiveWindow()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window.IsActive)
                return window;
        }
        return Application.Current.MainWindow;
    }
}
