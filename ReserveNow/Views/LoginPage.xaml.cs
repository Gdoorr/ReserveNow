using ReserveNow.ViewModels;
using Microsoft.Maui.Controls;
using ReserveNow;
using ReserveNow.Services;
using System.Text.Json;
using System.Text;

namespace ReserveNow
{

    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
          
        }
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                string username = UsernameEntry.Text;
                string password = PasswordEntry.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("������", "������� ����� � ������", "OK");
                    return;
                }

                var token = await LoginAsync(username, password);

                if (!string.IsNullOrEmpty(token))
                {
                    // ��������� ����� � SecureStorage
                    await SecureStorage.SetAsync("AccessToken", token);

                    // ��������� �� ������� ��������
                    await Navigation.PushAsync(new MainPage());
                }
                else
                {
                    await DisplayAlert("������", "�������� ������� ������", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", ex.Message, "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async Task<string> LoginAsync(string username, string password)
        {
            var client = new HttpClient();

            var loginData = new
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5000/api/Registration/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<dynamic>(responseString);

                return result.token; // ��������������, ��� ������ ���������� ����� � ���� "token"
            }

            return null;
        }
    }
}