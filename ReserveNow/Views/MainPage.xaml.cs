
using System.Collections.ObjectModel;
using ReserveNow.Models;
using Microsoft.Maui.Controls;
using System.Net.Http.Json;
using ReserveNow.Views;

namespace ReserveNow
{
    public partial class MainPage : ContentPage
    {
        private readonly string _baseUrl;
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        public  HttpClient _httpClient;
        private List<string> _cities = new();
        private List<Restaurant> _restaurants = new();
        public ObservableCollection<Restaurant> RecommendedRestaurants { get; set; }

        public MainPage(ApiService apiService, AuthService authService, HttpClient httpClient)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
            //LoadUserData();
            // Пример данных
            NavigationPage.SetHasNavigationBar(this, false);
           
            LoadData();
            LoadReservations();
            //LoadRestaurants();
            _httpClient = httpClient;
        }
        private async void LoadData()
        {
            try
            {
                //исправить здесь ошибка крашится при запуске
                // Загрузка городов
                _cities = await _apiService.GetCitiesAsync();
                CityPicker.ItemsSource = _cities;
                CityPicker.ItemDisplayBinding = new Binding(".");  // Отображаем название города

                // Установка города пользователя
                var userProfile = _authService.GetUserProfile();
                var userCity = userProfile.City;
                if (!string.IsNullOrEmpty(userCity) && _cities.Contains(userCity))
                {
                    CityPicker.SelectedItem = userCity;
                }

                // Загрузка ресторанов
                _restaurants = await _apiService.GetRestaurantsAsync();
                UpdateRestaurantsList(userCity);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var query = e.NewTextValue?.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                // Если строка поиска пустая, показываем все рестораны
                RestaurantsList.ItemsSource = _restaurants;
            }
            else
            {
                // Фильтруем рестораны по названию
                var filtered = _restaurants
                    .Where(r => r.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                RestaurantsList.ItemsSource = filtered;
            }
        }
        private async void LoadReservations()
        {
            try
            {
                var user = _authService.GetUserProfile();
                // Получаем список броней пользователя
                var reservations = await _apiService.MakeAuthorizedRequestAsync<List<Reservation>>(
                    $"{_baseUrl}/api/Restaurants/user/{user.ID}",
                    HttpMethod.Get
                );

                if (reservations != null && reservations.Any())
                {
                    var displayReservations = reservations.Select(r => new Reservation
                    {
                        
                        Date = DateTime.Parse(r.Date).ToString("yyyy-MM-dd"), // Форматируем дату
                        StartTime = TimeSpan.Parse(r.StartTime).ToString(@"hh\:mm"),   // Убираем секунды
                        EndTime = TimeSpan.Parse(r.EndTime).ToString(@"hh\:mm"),       // Убираем секунды
                        Guests = r.Guests
                    }).ToList();
                    // Отображаем список броней
                    ReservationsList.ItemsSource = displayReservations;
                    ReservationsList.IsVisible = true;
                    ReservationsLabel.IsVisible = true;

                }
                else
                {
                    // Показываем сообщение об отсутствии броней
                    ReservationsList.IsVisible = false;
                    ReservationsLabel.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load reservations: {ex.Message}", "OK");
            }
        }
        private void UpdateRestaurantsList(string selectedCity)
        {
            var sortedRestaurants = _restaurants
                .OrderByDescending(r => r.City == selectedCity) // Рестораны из выбранного города первыми
                .ThenBy(r => r.Name) // Дополнительная сортировка по имени
                .ToList();

            RestaurantsList.ItemsSource = sortedRestaurants;
        }

        private void OnCitySelected(object sender, EventArgs e)
        {
            var selectedCity = CityPicker.SelectedItem as string;
            UpdateRestaurantsList(selectedCity);
        }
        private async void OnRestaurantClicked(object sender, EventArgs e)
        {
            var restaurant = (sender as Button)?.BindingContext as Restaurant;
            if (restaurant != null)
            {
                var detailsPage = new RestaurantPage(_apiService,_authService, restaurant, _httpClient);
                await Navigation.PushAsync(detailsPage);
            }
        }
        private void OnSettingsClicked(object sender, EventArgs e)
        {
            // Ваш код здесь
            DisplayAlert("Info", "Settings button clicked!", "OK");
        }
        private void OnHomeClicked(object sender, EventArgs e)
        {
            // Ваш код здесь
            DisplayAlert("Info", "Settings button clicked!", "OK");
        }
        private void OnCancelBookingClicked(object sender, EventArgs e)
        {
            // Ваш код здесь
            DisplayAlert("Info", "Settings button clicked!", "OK");
        }
    }

}
