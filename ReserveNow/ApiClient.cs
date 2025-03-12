using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace ReserveNow
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly SecureStorage _secureStorage = new SecureStorage();

        public async Task<T> GetAsync<T>(string url)
        {
            await CheckAndRefreshToken();

            // Добавляем токен в заголовок
            var token = await _secureStorage.GetAsync("AccessToken");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<T>();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Если сервер вернул Unauthorized, очищаем токен и перезагружаем
                await _secureStorage.RemoveAsync("AccessToken");
                await Shell.Current.GoToAsync("//LoginPage");
            }

            throw new HttpRequestException($"Ошибка: {response.StatusCode}");
        }

        private async Task CheckAndRefreshToken()
        {
            var token = await _secureStorage.GetAsync("AccessToken");
            if (JwtHelper.IsTokenExpired(token))
            {
                // Реализуйте обновление токена через refresh token (если есть)
                // Если нет — очищаем токен и перенаправляем на LoginPage
                await _secureStorage.RemoveAsync("AccessToken");
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
