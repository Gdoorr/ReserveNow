
using Microsoft.Maui.Controls;
using ReserveNow;
using System.Text.Json;
using System.Text;
using ReserveNow.Views;
using System.Net.Http;
using System.Net.Http.Json;
using ReserveNow.Models;

namespace ReserveNow
{

    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService;
        private readonly ApiService _apiService;
        //private readonly HttpClient _httpClient;

        public LoginPage(ApiService apiService, AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                // �������� ��������� ������
                var username = UsernameEntry.Text;
                var password = PasswordEntry.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    StatusLabel.Text = "Please enter username and password.";
                    return;
                }

                // ������� �����������
                StatusLabel.Text = "Logging in...";
                var (accessToken, refreshToken) = await _authService.LoginAsync(username, password);
                if (accessToken!=null&&refreshToken!=null)
                {
                    StatusLabel.Text = "Login successful!";
                    var mainPage = new MainPage(new ApiService(new HttpClient(), _authService), new AuthService(new HttpClient()),new HttpClient());
                    await Navigation.PushAsync(mainPage);
                    Navigation.InsertPageBefore(mainPage, this); // �������� ������� ��������
                    await Navigation.PopAsync(); // ������� �������� ����� �� �����
                }
                else
                {
                    StatusLabel.Text = "Login failed!";
                }
                    // ���� �������, ���������� ���������
                  
            }
            catch (Exception ex)
            {
                // ��������� ������
                StatusLabel.Text = $"Error: {ex.Message}";
            }
        }
        
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // ������� �� �������� �����������
            await Navigation.PushAsync(new RegisterPage());
        }
        private async void OnForgotPasswordClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(
                new ResetPasswordPage());
        }
    }
}