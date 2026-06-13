using KPL_FE.Controllers;
using KPL_FE.Models;
using System.Windows;
using System.Windows.Media;

namespace KPL_FE.Views;

public partial class LoginPage : Window
{
    public bool Saved { get; private set; }

    public LoginPage()
    {
        InitializeComponent();
        LoginUsernameBox.TextChanged += (_, _) => UpdateLoginButton();
        LoginPasswordBox.PasswordChanged += (_, _) => UpdateLoginButton();
        RegUsernameBox.TextChanged += (_, _) => UpdateRegisterButton();
        RegPasswordBox.PasswordChanged += (_, _) => UpdateRegisterButton();
        RegDisplayNameBox.TextChanged += (_, _) => UpdateRegisterButton();
    }

    private void UpdateLoginButton()
    {
        LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(LoginUsernameBox.Text)
            && LoginPasswordBox.SecurePassword.Length > 0;
    }

    private void UpdateRegisterButton()
    {
        RegisterButton.IsEnabled = !string.IsNullOrWhiteSpace(RegUsernameBox.Text)
            && RegPasswordBox.SecurePassword.Length >= 6
            && !string.IsNullOrWhiteSpace(RegDisplayNameBox.Text);
    }

    private void ModeSwitch_Checked(object sender, RoutedEventArgs e)
    {
        if (LoginPanel is null) return;
        var isLogin = ModeLogin.IsChecked == true;
        LoginPanel.Visibility = isLogin ? Visibility.Visible : Visibility.Collapsed;
        RegisterPanel.Visibility = isLogin ? Visibility.Collapsed : Visibility.Visible;
        ClearStatus();
    }

    private string GetPassword(System.Windows.Controls.PasswordBox box)
    {
        var ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(box.SecurePassword);
        try
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr) ?? "";
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        SetEnabled(false);
        ClearStatus();

        var request = new LoginRequest
        {
            Username = LoginUsernameBox.Text.Trim(),
            Password = GetPassword(LoginPasswordBox)
        };

        try
        {
            var api = new AuthApiController();
            var response = await api.LoginAsync(request);
            ApplyAuth(response);
            Saved = true;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError($"Login gagal: {ex.Message}");
            SetEnabled(true);
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        SetEnabled(false);
        ClearStatus();

        if (RegPasswordBox.SecurePassword.Length < 6)
        {
            ShowError("Registrasi gagal: Password minimal 6 karakter.");
            SetEnabled(true);
            return;
        }

        var request = new RegisterRequest
        {
            Username = RegUsernameBox.Text.Trim(),
            Password = GetPassword(RegPasswordBox),
            DisplayName = RegDisplayNameBox.Text.Trim(),
            Role = (RegRoleCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Kasir"
        };

        try
        {
            var api = new AuthApiController();
            await api.RegisterAsync(request);

            LoginUsernameBox.Text = request.Username;
            LoginPasswordBox.Clear();
            RegPasswordBox.Clear();
            ModeLogin.IsChecked = true;
            ShowSuccess("Akun berhasil dibuat. Silakan login.");
            SetEnabled(true);
        }
        catch (Exception ex)
        {
            ShowError($"Registrasi gagal: {ex.Message}");
            SetEnabled(true);
        }
    }

    private static void ApplyAuth(LoginResponse response)
    {
        App.Token = response.Token;
        App.EmployeeId = response.EmployeeId;
        App.DisplayName = response.DisplayName;
        App.Role = response.Role;
    }

    private void SetEnabled(bool enabled)
    {
        CancelButton.IsEnabled = enabled;
        ModeLogin.IsEnabled = enabled;
        ModeRegister.IsEnabled = enabled;

        if (enabled)
        {
            UpdateLoginButton();
            UpdateRegisterButton();
            return;
        }

        LoginButton.IsEnabled = false;
        RegisterButton.IsEnabled = false;
    }

    private void ClearStatus()
    {
        ErrorText.Text = "";
    }

    private void ShowError(string message)
    {
        ErrorText.Foreground = Brushes.Red;
        ErrorText.Text = message;
    }

    private void ShowSuccess(string message)
    {
        ErrorText.Foreground = Brushes.Green;
        ErrorText.Text = message;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
