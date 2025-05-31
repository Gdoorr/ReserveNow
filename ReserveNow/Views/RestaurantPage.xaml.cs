using Microsoft.Maui.Controls;
using ReserveNow.Models;
namespace ReserveNow.Views;

public partial class RestaurantPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private Restaurant _restaurant;

    private readonly Reservation _reservation;
    private int _selectedTableId;

    public RestaurantPage(ApiService apiService, AuthService authService, Restaurant restaurant, HttpClient httpClient, Reservation reservation)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
        _restaurant = restaurant;

        BindingContext = _restaurant;
        _httpClient = httpClient;
        _reservation = reservation;
    }

    //private async void LoadTables()
    //{
    //    try
    //    {
    //        var tables = _restaurant.Tables;
    //        TablesList.ItemsSource = tables;
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", ex.Message, "OK");
    //    }
    //}

    private async void OnBookClicked(object sender, EventArgs e)
    {
        var bookingPage = new BookingPage(_apiService, _restaurant,_authService,_httpClient,_reservation);
        await Navigation.PushAsync(bookingPage);
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
        var editPage = new AccountSettingsPage(_apiService, _authService, _httpClient, _reservation);
        Navigation.PushAsync(editPage);
    }
}