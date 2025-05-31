
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
        private readonly string _baseUrl;
        private readonly Reservation _reservation;
        //private readonly HttpClient _httpClient;

        public LoginPage(ApiService apiService, AuthService authService, Reservation reservation)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
            _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
            _reservation = reservation;
            NavigationPage.SetHasNavigationBar(this, false);
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
                    StatusLabel.Text = "����������, ������� ��� ������������ � ������.";
                    return;
                }

                // ������� �����������
                StatusLabel.Text = "������ � �������...";
                var (accessToken, refreshToken) = await _authService.LoginAsync(username, password);
                if (accessToken!=null&&refreshToken!=null)
                {
                    StatusLabel.Text = "���� � ������� ������ �������!";
                    var mainPage = new MainPage(new ApiService(new HttpClient(), _authService), new AuthService(new HttpClient()),new HttpClient(),_reservation);
                    await Navigation.PushAsync(mainPage);
                    Navigation.InsertPageBefore(mainPage, this); // �������� ������� ��������
                    await Navigation.PopAsync(); // ������� �������� ����� �� �����
                }
                else
                {
                    StatusLabel.Text = "������ �����!";
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