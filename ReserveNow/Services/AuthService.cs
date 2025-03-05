using ReserveNow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace ReserveNow.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://yourapi.com/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LoginResponse>(responseString);
            }

            return null;
        }
    }
}
