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
        LoginUsernameBox.TextChanged += (_, _) => { UpdateLoginButton(); ClearStatus(); };
        LoginPasswordBox.PasswordChanged += (_, _) => { UpdateLoginButton(); ClearStatus(); };
        LoginPasswordTextBox.TextChanged += (_, _) => { UpdateLoginButton(); ClearStatus(); LoginPasswordBox.Password = LoginPasswordTextBox.Text; };
        LoginPasswordToggle.Checked += (_, _) => TogglePasswordVisibility(LoginPasswordBox, LoginPasswordTextBox, LoginPasswordToggle);
        LoginPasswordToggle.Unchecked += (_, _) => TogglePasswordVisibility(LoginPasswordBox, LoginPasswordTextBox, LoginPasswordToggle);
        RegUsernameBox.TextChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); };
        RegPasswordBox.PasswordChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); };
        RegPasswordTextBox.TextChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); RegPasswordBox.Password = RegPasswordTextBox.Text; };
        RegPasswordToggle.Checked += (_, _) => TogglePasswordVisibility(RegPasswordBox, RegPasswordTextBox, RegPasswordToggle);
        RegPasswordToggle.Unchecked += (_, _) => TogglePasswordVisibility(RegPasswordBox, RegPasswordTextBox, RegPasswordToggle);
        RegDisplayNameBox.TextChanged += (_, _) => { UpdateRegisterButton(); ClearStatus(); };
    }

    private void UpdateLoginButton()
    {
        var hasPassword = LoginPasswordTextBox.Visibility == Visibility.Visible
            ? !string.IsNullOrEmpty(LoginPasswordTextBox.Text)
            : LoginPasswordBox.SecurePassword.Length > 0;
        LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(LoginUsernameBox.Text) && hasPassword;
    }

    private void UpdateRegisterButton()
    {
        var hasPassword = RegPasswordTextBox.Visibility == Visibility.Visible
            ? !string.IsNullOrEmpty(RegPasswordTextBox.Text)
            : RegPasswordBox.SecurePassword.Length > 0;
        RegisterButton.IsEnabled = !string.IsNullOrWhiteSpace(RegUsernameBox.Text)
            && hasPassword
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

    private string GetActivePassword(System.Windows.Controls.PasswordBox box, System.Windows.Controls.TextBox textBox)
    {
        if (textBox.Visibility == Visibility.Visible)
            return textBox.Text;
        return GetPassword(box);
    }

    private static void TogglePasswordVisibility(System.Windows.Controls.PasswordBox passwordBox, System.Windows.Controls.TextBox textBox, System.Windows.Controls.Primitives.ToggleButton toggle)
    {
        if (toggle.IsChecked == true)
        {
            textBox.Text = passwordBox.Password;
            passwordBox.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.SelectionStart = textBox.Text.Length;
            toggle.ToolTip = "Sembunyikan password";
        }
        else
        {
            passwordBox.Password = textBox.Text;
            passwordBox.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Collapsed;
            passwordBox.Focus();
            toggle.ToolTip = "Tampilkan password";
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        SetEnabled(false);
        ClearStatus();

        var request = new LoginRequest
        {
            Username = LoginUsernameBox.Text.Trim(),
            Password = GetActivePassword(LoginPasswordBox, LoginPasswordTextBox)
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
        ClearStatus();

        var username = RegUsernameBox.Text.Trim();
        var displayName = RegDisplayNameBox.Text.Trim();
        var password = GetActivePassword(RegPasswordBox, RegPasswordTextBox);

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
            LoginPasswordTextBox.Clear();
            RegPasswordBox.Clear();
            RegPasswordTextBox.Clear();
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
