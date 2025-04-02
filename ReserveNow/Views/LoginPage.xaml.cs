
using Microsoft.Maui.Controls;
using ReserveNow;
using ReserveNow.Services;
using System.Text.Json;
using System.Text;

namespace ReserveNow
{

    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _authService = new AuthService(new HttpClient());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                StatusLabel.Text = "Please enter username and password.";
                return;
            }

            var isLoggedIn = await _authService.LoginAsync(username, password);
            if (isLoggedIn)
            {
                StatusLabel.Text = "Login successful!";
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                StatusLabel.Text = "Login failed.";
            }
        }
    }
}