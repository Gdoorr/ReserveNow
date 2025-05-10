
using System.Collections.ObjectModel;
using ReserveNow.Models;
using Microsoft.Maui.Controls;
using System.Net.Http.Json;
using ReserveNow.Views;

namespace ReserveNow
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        public  HttpClient _httpClient;
        private List<string> _cities = new();
        private List<Restaurant> _restaurants = new();
        public ObservableCollection<Restaurant> RecommendedRestaurants { get; set; }
        public ObservableCollection<Booking> UpcomingBookings { get; set; }

        public MainPage(ApiService apiService, AuthService authService, HttpClient httpClient)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            //LoadUserData();
            // Пример данных

            UpcomingBookings = new ObservableCollection<Booking>
            {
                new Booking { RestaurantName = "Уютный дом", BookingDate = "20 октября, 19:00" }
            };

            BindingContext = this;
            LoadData();
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
        private async void OnRestaurantTapped(object sender, EventArgs e)
        {
            var tappedRestaurant = (sender as TapGestureRecognizer)?.CommandParameter as Restaurant;
            if (tappedRestaurant != null)
            {
                await Navigation.PushAsync(new RestaurantPage(_apiService, _authService, tappedRestaurant));
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
