
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

        public async Task<T> MakeAuthorizedRequestAsync<T>(string url, HttpMethod method = null, object requestBody = null, Dictionary<string, string> queryParams = null)
        {
            // Получаем токены из сервиса авторизации
            var (accessToken, refreshToken, expireAt) = _authService.GetSavedTokens();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("No access token found");
            }

            // Добавляем параметры запроса к URL, если они есть
            if (queryParams != null && queryParams.Any())
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url = $"{url}?{query}";
            }

            // Создание HTTP-запроса
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, url);

            // Добавление заголовка авторизации
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            // Добавление тела запроса, если оно есть и метод поддерживает тело
            if (requestBody != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
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
                // Если тип ответа T - это string или byte[], возвращаем содержимое напрямую
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)await response.Content.ReadAsStringAsync();
                }
                else if (typeof(T) == typeof(byte[]))
                {
                    return (T)(object)await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
            }
            else
            {
                // Логируем ошибку и выбрасываем исключение
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request failed with status code: {response.StatusCode}. Response: {errorResponse}");
            }
        }
        public async Task<bool> CheckTableAvailabilityAsync(
    int restaurantId,
    int tableId,
    DateTime date,
    TimeSpan startTime,
    TimeSpan endTime,
    int guests,
    int? currentReservationId = null)
        {
            var url = $"{_baseUrl}/api/Reservations/check-availability?" +
                      $"restaurantId={restaurantId}&" +
                      $"tableId={tableId}&" +
                      $"date={date:yyyy-MM-dd}&" +
                      $"startTime={startTime:hh\\:mm\\:ss}&" +
                      $"endTime={endTime:hh\\:mm\\:ss}&" +
                      $"guests={guests}";

            if (currentReservationId.HasValue)
            {
                url += $"&currentReservationId={currentReservationId}";
            }

            var response = await MakeAuthorizedRequestAsync<bool>(url, HttpMethod.Get);
            return response;
        }
        public async Task UpdateReservationAsync(int reservationId, Reservation updatedReservation)
        {
            var url = $"{_baseUrl}/api/Restaurants/{reservationId}";
            await MakeAuthorizedRequestAsync<object>(url, HttpMethod.Put, updatedReservation);
        }
        public async Task<(TimeSpan OpenTime, TimeSpan CloseTime)> GetRestaurantWorkingHoursAsync(int restaurantId)
        {
            try
            {
                // Выполняем авторизованный GET-запрос
                var response = await MakeAuthorizedRequestAsync<WorkingHoursResponse>(
                    $"{_baseUrl}/api/Reservations/restaurants/{restaurantId}/working-hours",
                    HttpMethod.Get
                );

                if (response == null)
                {
                    throw new Exception("Failed to load working hours.");
                }

                // Преобразуем строки времени в TimeSpan
                return (TimeSpan.Parse(response.OpenTime), TimeSpan.Parse(response.CloseTime));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading working hours: {ex.Message}");
            }
        }
        public async Task UpdateUserAsync(Client updatedClient)
        {
            try
            {
                // Формируем URL для обновления данных пользователя
                var url = $"{_baseUrl}/api/Restaurants/users/{updatedClient.ID}";

                // Создаем HTTP-запрос
                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(updatedClient),
                        Encoding.UTF8,
                        "application/json"
                    )
                };

                // Добавляем заголовок авторизации
                var accessToken = Preferences.Get("AccessToken", null); // Получаем токен из локального хранилища
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is missing. Please log in again.");
                }

                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Отправляем запрос
                var response = await _httpClient.SendAsync(request);

                // Проверяем статус ответа
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update user profile. Status code: {response.StatusCode}. Response: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user profile: {ex.Message}");
            }
        }
        public async Task DeleteReservationAsync(int reservationId)
        {
            var url = $"{_baseUrl}/api/Restaurants/delete/{reservationId}";
            await MakeAuthorizedRequestAsync<object>(url, HttpMethod.Delete);
        }
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
        public async Task<List<Reservation>> GetUserReservationsAsync(int userId)
        {
            var url = $"{_baseUrl}/api/Reservations/user/{userId}";
            return await MakeAuthorizedRequestAsync<List<Reservation>>(url, HttpMethod.Get);
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
