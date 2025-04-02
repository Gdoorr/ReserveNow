using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public ApiService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
        {
            var accessToken = await TokenStorage.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(accessToken) || JwtHelper.IsTokenExpired(accessToken))
            {
                var isRefreshed = await _authService.RefreshTokenAsync();
                if (!isRefreshed)
                {
                    throw new Exception("Failed to refresh token.");
                }

                accessToken = await TokenStorage.GetAccessTokenAsync();
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            return await _httpClient.SendAsync(request);
        }
    }
}
