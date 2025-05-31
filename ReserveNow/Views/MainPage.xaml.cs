
using System.Collections.ObjectModel;
using ReserveNow.Models;
using Microsoft.Maui.Controls;
using System.Net.Http.Json;
using ReserveNow.Views;
using System.Windows.Input;
using System.Threading.Tasks;

namespace ReserveNow.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly string _baseUrl;
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private Restaurant _restaurant;
        public  HttpClient _httpClient;
        private List<string> _cities = new();
        private List<Restaurant> _restaurants = new();
        private ObservableCollection<Reservation> _reservations;
        private readonly Reservation _reservation;
        private bool _isDataLoaded = false;
        
        public ObservableCollection<Restaurant> RecommendedRestaurants { get; set; }

        public MainPage(ApiService apiService, AuthService authService, HttpClient httpClient, Reservation reservation)
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
            _reservation = reservation;
            
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
                _isDataLoaded = true;
                UpdateRestaurantsList();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        private void UpdateRestaurantsList()
        {
            if (!_isDataLoaded) return;
            try
            {
                // Получаем выбранный город
                var selectedCity = CityPicker.SelectedItem?.ToString();

                // Получаем текст из поля поиска
                var searchText = SearchEntry.Text?.Trim();

                // Фильтруем рестораны
                var filteredRestaurants = _restaurants
                    .Where(r =>
                        (string.IsNullOrEmpty(selectedCity) || r.City == selectedCity) && // Фильтр по городу
                        (string.IsNullOrEmpty(searchText) || r.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) // Фильтр по тексту поиска
                    )
                    .ToList();

                // Обновляем список ресторанов
                RestaurantsList.ItemsSource = filteredRestaurants;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to filter restaurants: {ex.Message}", "OK");
            }
        }
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRestaurantsList();
            //var query = e.NewTextValue?.Trim();

            //if (string.IsNullOrWhiteSpace(query))
            //{
            //    // Если строка поиска пустая, показываем все рестораны
            //    RestaurantsList.ItemsSource = _restaurants;
            //}
            //else
            //{
            //    // Фильтруем рестораны по названию
            //    var filtered = _restaurants
            //        .Where(r => r.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            //        .ToList();

            //    RestaurantsList.ItemsSource = filtered;
            //}
        }
        private async void OnEditReservationClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null || button.BindingContext == null)
            {
                await DisplayAlert("Ошибка", "Не удалось получить информацию о бронировании.", "Ок");
                return;
            }

            // Получаем бронь из BindingContext
            var reservation = button.BindingContext as ReservationRequest;
            if (reservation == null)
            {
                await DisplayAlert("Ошибка", "Данные о бронировании отсутствуют.", "Ок");
                return;
            }
            // Подтверждение действия
            var confirm = await DisplayAlert("Потвердите", "Вы уверены, что хотите изменить это бронирование?", "Да", "Нет");
            if (!confirm) return;

            // Переход на страницу редактирования
            var editPage = new EditReservation(reservation,_apiService,_authService,_httpClient);
            await Navigation.PushAsync(editPage);

            // После возврата с страницы редактирования обновляем список
            LoadReservations();
        }

        private async void OnDeleteReservationClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null || button.BindingContext == null)
            {
                await DisplayAlert("Ошибка", "Не удалось получить информацию о бронировании.", "Ок");
                return;
            }

            // Получаем бронь из BindingContext
            var reservation = button.BindingContext as ReservationRequest;
            if (reservation == null)
            {
                await DisplayAlert("Ошибка", "Данные о бронировании отсутствуют.", "Ок");
                return;
            }
            // Подтверждение действия
            var confirm = await DisplayAlert("Потвердите", "Вы уверены, что хотите удалить это бронирование?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                await _apiService.DeleteReservationAsync(reservation.ID);
                //_reservations.Remove(_reservation);
                await DisplayAlert("Успех", "Бронирование успешно удалено.", "Ок");
                var bookingPage = new MainPage(_apiService, _authService, _httpClient, _reservation);
                await Navigation.PushAsync(bookingPage);

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete reservation: {ex.Message}", "Ок");
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
                    var displayReservations = reservations.Select(r => new ReservationRequest
                    {
                        ID=r.ID,
                        UserId=r.UserId,
                        RestaurantId=r.RestaurantId,
                        RestaurantName = r.RestaurantName,
                        RestaurantCity=r.RestaurantCity,
                        TableId=r.TableId,
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
        //private void UpdateRestaurantsList(string selectedCity)
        //{
        //    var sortedRestaurants = _restaurants
        //        .OrderByDescending(r => r.City == selectedCity) // Рестораны из выбранного города первыми
        //        .ThenBy(r => r.Name) // Дополнительная сортировка по имени
        //        .ToList();

        //    RestaurantsList.ItemsSource = sortedRestaurants;
        //}

        private void OnCitySelected(object sender, EventArgs e)
        {
            UpdateRestaurantsList();
            //var selectedCity = CityPicker.SelectedItem as string;
            //UpdateRestaurantsList(selectedCity);
        }
        private async void OnRestaurantClicked(object sender, EventArgs e)
        {
            var restaurant = (sender as Button)?.BindingContext as Restaurant;
            if (restaurant != null)
            {
                var detailsPage = new RestaurantPage(_apiService,_authService, restaurant, _httpClient, _reservation);
                await Navigation.PushAsync(detailsPage);
            }
        }
        private void OnHomeClicked(object sender, EventArgs e)
        {
            var editPage = new MainPage(_apiService, _authService, _httpClient, _reservation);
            Navigation.PushAsync(editPage);
        }

        private void OnHistoryClicked(object sender, EventArgs e)
        {
            var UserData = _authService.GetUserProfile();
            var editPage = new ReservationHistoryPage(_apiService, UserData.ID, _authService, _httpClient, _reservation);
            Navigation.PushAsync(editPage);
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            var editPage = new AccountSettingsPage(_apiService, _authService, _httpClient, _reservation );
            Navigation.PushAsync(editPage);
        }
    }

}
