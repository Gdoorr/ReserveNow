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
            SecureStorage.RemoveAll();
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
                    // ���������, ����� �� �����
                    if (IsTokenExpired(token))
                    {
                        await DisplayAlert("������", "���������� ����� ��� �����. ����������, ��������� �������.", "OK");
                        return;
                    }

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
                Email = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5000/api/Registration/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(responseString);

                // ���������, ���������� �� �������� "token" � ������
                if (jsonResponse.RootElement.TryGetProperty("token", out var tokenProperty))
                {
                    return tokenProperty.GetString(); // �������� �������� ������
                }
            }

            return null;
        }
        private bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true; // ���� ����� ������, ������� ��� ����������������
            }

            try
            {
                // ��������� ����� �� �����
                var parts = token.Split('.');
                if (parts.Length < 2)
                {
                    return true; // �������� ������ ������
                }

                // ����� ������ ����� (payload) � ���������� �
                var payloadBase64 = parts[1];
                var payloadJson = DecodeBase64(payloadBase64);

                // ������ JSON-������
                var payload = JsonDocument.Parse(payloadJson);
                if (!payload.RootElement.TryGetProperty("exp", out var expProperty))
                {
                    return true; // ���� ��� ���� 'exp', ������� ����� ����������������
                }

                // �������� ����� ���������
                if (expProperty.ValueKind != JsonValueKind.Number)
                {
                    return true; // ���� 'exp' ������ ���� ������
                }

                var expirationTimeUnix = expProperty.GetInt64();
                var expirationDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expirationTimeUnix).UtcDateTime;

                // ���������� � ������� ��������
                var currentTimeUtc = DateTime.UtcNow;
                return currentTimeUtc >= expirationDateTimeUtc; // true, ���� ����� �����
            }
            catch
            {
                return true; // ��� ����� ������ ������� ����� ����������������
            }
        }

        private string DecodeBase64(string base64String)
        {
            // ��������� ����������� '=' ��� ����������� Base64
            base64String += new string('=', base64String.Length % 4);
            var bytes = Convert.FromBase64String(base64String);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}