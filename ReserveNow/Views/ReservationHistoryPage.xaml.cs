using ReserveNow.Models;

namespace ReserveNow.Views;

public partial class ReservationHistoryPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly int _userId;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly Reservation _reservation;
    public ReservationHistoryPage(ApiService apiService, int userId,AuthService authService,HttpClient httpClient,Reservation reservation)
	{
		InitializeComponent();
        _apiService = apiService;
        _userId = userId;
        _authService = authService;
        _httpClient = httpClient;
        _reservation = reservation;
        _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
        LoadReservations();
    }
    private async void LoadReservations()
    {
        try
        {
            var user = _authService.GetUserProfile();
            // Получаем список броней пользователя
            var reservations = await _apiService.MakeAuthorizedRequestAsync<List<Reservation>>(
                $"{_baseUrl}/api/Restaurants/history/{user.ID}",
                HttpMethod.Get
            );

            if (reservations != null && reservations.Any())
            {
                var displayReservations = reservations.Select(r => new ReservationRequest
                {
                    ID = r.ID,
                    UserId = r.UserId,
                    RestaurantId = r.RestaurantId,
                    RestaurantName = r.RestaurantName,
                    RestaurantCity = r.RestaurantCity,
                    TableId = r.TableId,
                    Date = DateTime.Parse(r.Date).ToString("yyyy-MM-dd"), // Форматируем дату
                    StartTime = TimeSpan.Parse(r.StartTime).ToString(@"hh\:mm"),   // Убираем секунды
                    EndTime = TimeSpan.Parse(r.EndTime).ToString(@"hh\:mm"),       // Убираем секунды
                    Guests = r.Guests
                }).ToList();
                // Отображаем список броней
                ReservationsList.ItemsSource = displayReservations;
                ReservationsList.IsVisible = true;
                

            }
            else
            {
                // Показываем сообщение об отсутствии броней
                ReservationsList.IsVisible = false;
               
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load reservations: {ex.Message}", "OK");
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
        var editPage = new ReservationHistoryPage(_apiService, UserData.ID,_authService,_httpClient,_reservation);
        Navigation.PushAsync(editPage);
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        var editPage = new AccountSettingsPage(_apiService, _authService, _httpClient, _reservation);
        Navigation.PushAsync(editPage);
    }
}