using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string LoginEndpoint = "http://localhost:5000/api/Registration/login";
        private const string RefreshEndpoint = "https://yourapi.com/refresh-token";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync(LoginEndpoint, new { username, password });

            if (response.IsSuccessStatusCode)
            {
                var tokens = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (tokens != null && tokens.ContainsKey("accessToken") && tokens.ContainsKey("refreshToken"))
                {
                    await TokenStorage.SaveAccessTokenAsync(tokens["accessToken"]);
                    await TokenStorage.SaveRefreshTokenAsync(tokens["refreshToken"]);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await TokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false; // Нет refresh токена
            }

            var response = await _httpClient.PostAsJsonAsync(RefreshEndpoint, new { refreshToken });

            if (response.IsSuccessStatusCode)
            {
                var tokens = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (tokens != null && tokens.ContainsKey("accessToken") && tokens.ContainsKey("refreshToken"))
                {
                    await TokenStorage.SaveAccessTokenAsync(tokens["accessToken"]);
                    await TokenStorage.SaveRefreshTokenAsync(tokens["refreshToken"]);
                    return true;
                }
            }

            return false;
        }
    }
}
