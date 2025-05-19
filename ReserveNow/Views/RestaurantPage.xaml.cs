using Microsoft.Maui.Controls;
using ReserveNow.Models;
namespace ReserveNow.Views;

public partial class RestaurantPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private Restaurant _restaurant;
    private int _selectedTableId;

    public RestaurantPage(ApiService apiService, AuthService authService, Restaurant restaurant, HttpClient httpClient)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
        _restaurant = restaurant;

        BindingContext = _restaurant;
        _httpClient = httpClient;
    }

    private async void LoadTables()
    {
        try
        {
            var tables = _restaurant.Tables;
            TablesList.ItemsSource = tables;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBookClicked(object sender, EventArgs e)
    {
        var bookingPage = new BookingPage(_apiService, _restaurant,_authService,_httpClient);
        await Navigation.PushAsync(bookingPage);
    }
}