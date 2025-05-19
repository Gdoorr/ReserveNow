
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
    public class ApiService
    {
        public readonly HttpClient _httpClient;
        public readonly AuthService _authService;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authService = authService;
            _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
        }

        public async Task<T> MakeAuthorizedRequestAsync<T>(string url, HttpMethod method = null, object requestBody = null)
        {
            var (accessToken, refreshToken,expireAt) = _authService.GetSavedTokens();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("No access token found");
            }

            // Создание HTTP-запроса
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, url);

            // Добавление заголовка авторизации
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            // Добавление тела запроса, если оно есть
            if (requestBody != null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );
            }

            // Отправка запроса
            var response = await _httpClient.SendAsync(request);

            // Обработка ошибки 401 Unauthorized
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                try
                {
                    // Обновляем токен
                    accessToken = await _authService.RefreshAccessTokenAsync(refreshToken);

                    // Повторная отправка запроса с новым токеном
                    request.Headers.Remove("Authorization");
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");
                    response = await _httpClient.SendAsync(request);
                }
                catch
                {
                    // Если обновление токена не удалось, очищаем токены
                    _authService.ClearTokens();
                    throw new Exception("Authorization failed. Please log in again.");
                }
            }

            // Обработка успешного ответа
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            else
            {
                throw new Exception($"Request failed with status code: {response.StatusCode}");
            }
        }
        //public async Task<Client> FetchUserDataAsync()
        //{
        //    var json = await MakeAuthorizedRequestAsync("http://192.168.1.5:5000/api/Registretion/data");
        //    return JsonSerializer.Deserialize<Client>(json);
        //}
        public async Task<List<string>> GetCitiesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/Registration/cities");
            if (response.IsSuccessStatusCode)
            {
                var cityModels = await response.Content.ReadFromJsonAsync<List<City>>();
                return cityModels.Select(city => city.Name).ToList();
            }
            else
            {
                throw new Exception("Failed to load cities");
            }
        }

        public async Task<List<Restaurant>> GetRestaurantsAsync()
        {
            return await MakeAuthorizedRequestAsync<List<Restaurant>>($"{_baseUrl}/api/Restaurants/list");
        }
        public async Task<Restaurant> GetRestaurantAsync(int id)
        {
            return await MakeAuthorizedRequestAsync<Restaurant>($"{_baseUrl}/api/Restaurants/{id}");
        }
       public async Task<Table> GetAvailableTablesAsync(
        int restaurantId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        int guests)
        {
            var url = $"{_baseUrl}/api/Restaurants/{restaurantId}/available-tables" +
                      $"?date={date:yyyy-MM-dd}" +
                      $"&startTime={startTime:hh\\:mm}" +
                      $"&endTime={endTime:hh\\:mm}" +
                      $"&guests={guests}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var availableTables = await response.Content.ReadFromJsonAsync<List<Table>>();

                // Выбираем столик с минимальной вместимостью, но достаточной для размещения всех гостей
                var bestTable = availableTables
                    .OrderBy(t => t.Capacity) // Сортируем по вместимости
                    .FirstOrDefault();       // Берем первый (минимальный)

                return bestTable;
            }
            throw new Exception("Failed to load available tables");
        }

        public async Task ReserveTableAsync(int restaurantId, int tableId, DateTime date, TimeSpan time, int guests)
        {
            var reservationRequest = new
            {
                UserId = 1, // Замените на ID текущего пользователя
                RestaurantId = restaurantId,
                TableId = tableId,
                Date = date,
                Time = time,
                Guests = guests
            };

            await MakeAuthorizedRequestAsync<object>(
                $"{_baseUrl}/api/Restaurants/reserve ",
                HttpMethod.Post,
                reservationRequest
            );
        }
        
    }
}
