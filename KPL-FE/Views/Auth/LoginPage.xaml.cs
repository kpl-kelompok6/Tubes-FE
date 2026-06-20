using KPL_FE.Controllers;
using KPL_FE.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace KPL_FE.Views;

public partial class LoginPage : Window
{
    public bool Saved { get; private set; }

    public LoginPage()
    {
        InitializeComponent();
        LoginUsernameBox.TextChanged += (_, _) => { UpdateLoginButton(); ClearStatus(); };
        LoginPasswordBox.PasswordChanged += (_, _) => { UpdateLoginButton(); ClearStatus(); };
        RegUsernameBox.TextChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); };
        RegPasswordBox.PasswordChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); };
        RegDisplayNameBox.TextChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); };
    }

    private void UpdateLoginButton()
    {
        LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(LoginUsernameBox.Text) && LoginPasswordBox.SecurePassword.Length > 0;
    }

    private void UpdateRegisterButton()
    {
        RegisterButton.IsEnabled = !string.IsNullOrWhiteSpace(RegUsernameBox.Text)
            && RegPasswordBox.SecurePassword.Length > 0
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
            ShowError(GetFriendlyErrorMessage(ex, "login"));
            SetEnabled(true);
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        ClearStatus();

        var username = RegUsernameBox.Text.Trim();
        var displayName = RegDisplayNameBox.Text.Trim();
        var password = GetPassword(RegPasswordBox);

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Registrasi gagal: Username tidak boleh kosong.");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Registrasi gagal: Password minimal 6 karakter.");
            return;
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            ShowError("Registrasi gagal: Display Name tidak boleh kosong.");
            return;
        }

        SetEnabled(false);

        var request = new RegisterRequest
        {
            Username = username,
            Password = password,
            DisplayName = displayName,
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
            ShowError(GetFriendlyErrorMessage(ex, "registrasi"));
            SetEnabled(true);
        }
    }

    private static string GetFriendlyErrorMessage(Exception ex, string context)
    {
        if (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
            return $"{context} gagal: Server tidak merespons. Coba lagi.";

        if (ex is HttpRequestException)
            return $"{context} gagal: Tidak dapat terhubung ke server. Coba lagi.";

        return $"{context} gagal: {ex.Message}";
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
        LoginUsernameBox.IsEnabled = enabled;
        LoginPasswordBox.IsEnabled = enabled;
        RegUsernameBox.IsEnabled = enabled;
        RegPasswordBox.IsEnabled = enabled;
        RegDisplayNameBox.IsEnabled = enabled;
        RegRoleCombo.IsEnabled = enabled;

        if (enabled)
        {
            AuthLoadingPanel.Visibility = Visibility.Collapsed;
            AuthLoadingRing.IsActive = false;
            UpdateLoginButton();
            UpdateRegisterButton();
            return;
        }

        LoginButton.IsEnabled = false;
        RegisterButton.IsEnabled = false;
        AuthLoadingPanel.Visibility = Visibility.Visible;
        AuthLoadingRing.IsActive = true;
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
