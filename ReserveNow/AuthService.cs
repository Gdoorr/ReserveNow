
using ReserveNow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReserveNow
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl= MauiProgram.Configuration["ServerSettings:BaseUrl"];
        private static string LoginEndpoint = "http://192.168.1.5:5000/api/Registration/login";
        private static string RefreshEndpoint = "http://192.168.1.5:5000/api/Registration/refresh-token";
        private static string ClientData = "http://192.168.1.5:5000/api/Registration/data";
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new Exception("Base URL is not set in configuration.");
            }
        }
        public async Task<(string AccessToken, string RefreshToken)> LoginAsync(string email, string password)
        {
            try
            {
                Console.WriteLine("Sending request to API...");
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Registration/login", new { email, password });
                var data = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Registration/data", new { email });
                if (response.IsSuccessStatusCode)
                {
                    var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    var profile = await data.Content.ReadFromJsonAsync<Client>();
                    SaveTokens(tokens.AccessToken, tokens.RefreshToken,tokens.ExpireAt);
                    SaveUserProfile(profile);
                    return (tokens.AccessToken, tokens.RefreshToken);
                }
                else
                {
                    ClearTokens();
                    var (accessToken, refreshToken, expireAt) = GetSavedTokens();
                    ClearUserProfile();
                    return (accessToken, refreshToken);
                    //throw new Exception("Login failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return (null, null);
            }
        }
        public async Task<string> FetchDataFromApi()
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{_baseUrl}/api/example");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Registration/refresh-token", new { refreshToken });
            if (response.IsSuccessStatusCode)
            {
                var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
                var (accessToken, refresh, expireAt) = GetSavedTokens();
                SaveTokens(tokens.AccessToken, refreshToken,expireAt); // Обновляем только access_token
                return tokens.AccessToken;
            }
            else
            {
                ClearTokens();
                ClearUserProfile();
                throw new Exception("Token refresh failed");
                
            }
        }
        public async Task<bool> ValidateRefreshTokenAsync(string RefreshToken)
        {
            var json = JsonSerializer.Serialize(new { RefreshToken });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Registration/validate", content);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Invalid refresh token");
            }
            return await response.Content.ReadFromJsonAsync<bool>();
        }
        //public async Task<string> RefreshAccessToken(string refreshToken)
        //{
        //    var response = await _httpClient.PostAsJsonAsync(RefreshEndpoint, new { refreshToken });
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        //        SaveTokens(tokens.AccessToken, refreshToken); // Обновляем только access_token
        //        return tokens.AccessToken;
        //    }
        //    else
        //    {
        //        ClearTokens();
        //        ClearUserProfile();
        //        throw new Exception("Token refresh failed");

        //    }
        //}
        public (string AccessToken, string RefreshToken, DateTime ExpireAt) GetSavedTokens()
        {
            var accessToken = Preferences.Get("AccessToken", null);
            var refreshToken = Preferences.Get("RefreshToken", null);
            var expireAt = Preferences.Get("ExpireAt", null);
            DateTime.TryParse(expireAt, out DateTime exp);
            return (accessToken, refreshToken, exp);
        }
        public void ClearTokens()
        {
            Preferences.Remove("AccessToken");
            Preferences.Remove("RefreshToken");
            Preferences.Remove("ExpireAt");
            Preferences.Clear();
        }
        public void SaveTokens(string accessToken, string refreshToken, DateTime ExpireAt)
        {
            Preferences.Set("AccessToken", accessToken);
            Preferences.Set("RefreshToken", refreshToken);
            Preferences.Set("ExpireAt", ExpireAt.ToString("o"));
        }
        public void SaveUserProfile(Client profile)
        {
            var json = JsonSerializer.Serialize(profile);
            Preferences.Set("Client", json);
        }

        public Client GetUserProfile()
        {
            var json = Preferences.Get("Client", null);
            return json != null ? JsonSerializer.Deserialize<Client>(json) : null;
        }

        public void ClearUserProfile()
        {
            Preferences.Remove("UserProfile");
            Preferences.Clear();
        }
        //public async Task<bool> LoginAsync(string email, string password)
        //{
        //    var response = await _httpClient.PostAsJsonAsync(LoginEndpoint, new { email, password });

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var data = await _httpClient.PostAsJsonAsync(ClientData,new { email});
        //        // Игнорируем токены, просто возвращаем успешный результат
        //        var UserData = data.Content.ReadAsStringAsync();
        //        AppData.CurrentUser = 
        //        new Client
        //        {
        //            ID = UserData.Id,
        //            Name = UserData.name,
        //            Email = email,
        //            Phone = "+79991234567",
        //            Role = "User",
        //            CityId = 1
        //        };
        //    }

        //    return false;
        //}

        //public async Task<bool> RefreshTokenAsync()
        //{
        //    var refreshToken = await TokenStorage.GetRefreshTokenAsync();
        //    if (string.IsNullOrEmpty(refreshToken))
        //    {
        //        return false; // Нет refresh токена
        //    }

        //    var response = await _httpClient.PostAsJsonAsync(RefreshEndpoint, new { refreshToken });

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var tokens = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        //        if (tokens != null && tokens.ContainsKey("accessToken") && tokens.ContainsKey("refreshToken"))
        //        {
        //            await TokenStorage.SaveAccessTokenAsync(tokens["accessToken"]);
        //            await TokenStorage.SaveRefreshTokenAsync(tokens["refreshToken"]);
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}
