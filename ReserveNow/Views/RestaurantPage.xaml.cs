using Microsoft.Maui.Controls;
using ReserveNow.Models;
namespace ReserveNow.Views;

public partial class RestaurantPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private Restaurant _restaurant;
    private int _selectedTableId;

    public RestaurantPage(ApiService apiService, AuthService authService, Restaurant restaurant)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
        _restaurant = restaurant;

        BindingContext = _restaurant;
    }

    private async void OnCheckTablesClicked(object sender, EventArgs e)
    {
        try
        {
            var selectedDate = DatePicker.Date;
            var selectedTime = TimePicker.Time;

            var tables = await _apiService.GetAvailableTablesAsync(_restaurant.ID, selectedDate, selectedTime);
            TablesList.ItemsSource = tables;
            TablesList.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBookTableClicked(object sender, EventArgs e)
    {
        var table = (sender as Button)?.BindingContext as dynamic;
        if (table == null)
        {
            await DisplayAlert("Error", "No table selected", "OK");
            return;
        }

        try
        {
            await _apiService.ReserveTableAsync(
                _restaurant.ID,
                table.ID,
                DatePicker.Date,
                TimePicker.Time,
                table.Capacity
            );

            await DisplayAlert("Success", "Reservation confirmed", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}