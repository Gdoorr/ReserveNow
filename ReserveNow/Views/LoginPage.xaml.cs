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
                    await DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                    return;
                }

                var token = await LoginAsync(username, password);

                if (!string.IsNullOrEmpty(token))
                {
                    // Проверяем, истек ли токен
                    if (IsTokenExpired(token))
                    {
                        await DisplayAlert("Ошибка", "Полученный токен уже истек. Пожалуйста, повторите попытку.", "OK");
                        return;
                    }

                    // Сохраняем токен в SecureStorage
                    await SecureStorage.SetAsync("AccessToken", token);
                    // Переходим на главную страницу
                    await Navigation.PushAsync(new MainPage());
                }
                else
                {
                    await DisplayAlert("Ошибка", "Неверные учетные данные", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
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

                // Проверяем, существует ли свойство "token" в ответе
                if (jsonResponse.RootElement.TryGetProperty("token", out var tokenProperty))
                {
                    return tokenProperty.GetString(); // Получаем значение токена
                }
            }

            return null;
        }
        private bool IsTokenExpired(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return true; // Если токен пустой, считаем его недействительным
            }

            try
            {
                // Разделяем токен на части
                var parts = token.Split('.');
                if (parts.Length < 2)
                {
                    return true; // Неверный формат токена
                }

                // Берем вторую часть (payload) и декодируем её
                var payloadBase64 = parts[1];
                var payloadJson = DecodeBase64(payloadBase64);

                // Парсим JSON-строку
                var payload = JsonDocument.Parse(payloadJson);
                if (!payload.RootElement.TryGetProperty("exp", out var expProperty))
                {
                    return true; // Если нет поля 'exp', считаем токен недействительным
                }

                // Получаем время истечения
                if (expProperty.ValueKind != JsonValueKind.Number)
                {
                    return true; // Поле 'exp' должно быть числом
                }

                var expirationTimeUnix = expProperty.GetInt64();
                var expirationDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expirationTimeUnix).UtcDateTime;

                // Сравниваем с текущим временем
                var currentTimeUtc = DateTime.UtcNow;
                return currentTimeUtc >= expirationDateTimeUtc; // true, если токен истек
            }
            catch
            {
                return true; // При любой ошибке считаем токен недействительным
            }
        }

        private string DecodeBase64(string base64String)
        {
            // Добавляем недостающие '=' для корректного Base64
            base64String += new string('=', base64String.Length % 4);
            var bytes = Convert.FromBase64String(base64String);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}