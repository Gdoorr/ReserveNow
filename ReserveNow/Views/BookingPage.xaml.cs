using ReserveNow.Models;
using Microsoft.Maui.Controls;
namespace ReserveNow.Views;

public partial class BookingPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Restaurant _restaurant;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private Table _selectedTable;
    private readonly string _baseUrl;
    public BookingPage(ApiService apiService, Restaurant restaurant, AuthService authService,HttpClient httpClient)
	{
		InitializeComponent();

        _apiService = apiService;
        _restaurant = restaurant;
        _authService = authService;
        _httpClient = httpClient;
        _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
        LoadRestaurantDetails();
    }
    private async void LoadRestaurantDetails()
    {
        try
        {
            // �������� ���������� � ���������
            var restaurant = await _apiService.GetRestaurantAsync(_restaurant.ID);

            // ������������ ����� �������
            StartTimePicker.MinimumTime = restaurant.OpeningTime;
            StartTimePicker.MaximumTime = restaurant.ClosingTime;

            EndTimePicker.MinimumTime = restaurant.OpeningTime;
            EndTimePicker.MaximumTime = restaurant.ClosingTime;
            Console.WriteLine($"Opening Time: {restaurant.OpeningTime}, Closing Time: {restaurant.ClosingTime}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private async void OnCheckAvailabilityClicked(object sender, EventArgs e)
    {
        try
        {
            var date = DatePicker.Date;

            // �������� ��������� ����� �� ���������� TimePicker
            var startTimeString = StartTimePicker.SelectedItem?.ToString();
            var endTimeString = EndTimePicker.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
            {
                await DisplayAlert("Error", "Please select both start and end times.", "OK");
                return;
            }

            var startTime = TimeSpan.Parse(startTimeString);
            var endTime = TimeSpan.Parse(endTimeString);

            var guests = int.Parse(GuestsEntry.Text);

            var availableTables = await _apiService.GetAvailableTablesAsync(
                _restaurant.ID, date, startTime, endTime, guests);

            if (availableTables == null)
            {
                // ��� ��������� ������
                AvailabilityStatus.Text = "No available tables for this time range.";
                AvailabilityStatus.TextColor = Colors.Red;
                AvailabilityStatus.IsVisible = true;
                ConfirmBookingButton.IsVisible = false; // �������� ������ �������������
            }
            else
            {
                // ������ ��������� ������
                _selectedTable = availableTables;
                AvailabilityStatus.Text = $"Available table found: {_selectedTable.Capacity} seats.";
                AvailabilityStatus.TextColor = Colors.Green;
                AvailabilityStatus.IsVisible = true;
                ConfirmBookingButton.IsVisible = true; // ���������� ������ �������������
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private void OnStartTimeChanged(object sender, EventArgs e)
    {
        // �������� ��������� ����� �� StartTimePicker
        var startTimeString = StartTimePicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(startTimeString))
        {
            return;
        }

        var startTime = TimeSpan.Parse(startTimeString);

        // ������������� ����������� ����� ��� EndTimePicker
        EndTimePicker.MinimumTime = startTime.Add(TimeSpan.FromMinutes(30));
        EndTimePicker.MaximumTime = EndTimePicker.MaximumTime; // ��������� ������������ ����� ��� ���������

        // ���������, ��� EndTimePicker �� �������� ������������ �����
        var endTimeString = EndTimePicker.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(endTimeString))
        {
            var endTime = TimeSpan.Parse(endTimeString);
            if (endTime <= startTime)
            {
                ValidationMessage.Text = "End time must be later than start time.";
                EndTimePicker.SelectedItem = null; // ������� �����
            }
            else
            {
                ValidationMessage.Text = string.Empty;
            }
        }
    }

    private void OnEndTimeChanged(object sender, EventArgs e)
    {
        // �������� ��������� �������
        var startTimeString = StartTimePicker.SelectedItem?.ToString();
        var endTimeString = EndTimePicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
        {
            return;
        }

        var startTime = TimeSpan.Parse(startTimeString);
        var endTime = TimeSpan.Parse(endTimeString);

        // ���������, ��� EndTime ������ StartTime
        if (endTime <= startTime)
        {
            ValidationMessage.Text = "End time must be later than start time.";
            EndTimePicker.SelectedItem = null; // ������� �����
        }
        else
        {
            ValidationMessage.Text = string.Empty;
        }
    }
    private async void OnConfirmReservationClicked(object sender, EventArgs e)
    {
        try
        {
            var userProfile = _authService.GetUserProfile();
            var startTimeString = StartTimePicker.SelectedItem?.ToString();
            var endTimeString = EndTimePicker.SelectedItem?.ToString();

            // ���������, ������� �� ��������� � �������� �����
            if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
            {
                await DisplayAlert("Error", "Please select both start and end times.", "OK");
                return;
            }

            // ����������� ���� � UTC � ������ ISO 8601
            var dateUtc = DatePicker.Date.ToUniversalTime(); // ����������� � UTC
            var dateIso8601 = dateUtc.ToString("yyyy-MM-dd"); // ������ ISO 8601

            // ��������� ���������� ������
            if (!int.TryParse(GuestsEntry.Text, out var guests) || guests <= 0)
            {
                await DisplayAlert("Error", "Please enter a valid number of guests.", "OK");
                return;
            }

            // ��������� ������ �������
            var reservationRequest = new Reservation
            {
                UserId = userProfile.ID, // ID ������������
                RestaurantId = _restaurant.ID,
                TableId = _selectedTable.ID,
                Date = dateIso8601, // ���� � ������� ISO 8601 � �������� UTC
                StartTime = startTimeString + ":00", // ����� � ������� ������ hh:mm:ss
                EndTime = endTimeString + ":00",    // ����� � ������� ������ hh:mm:ss
                Guests = guests
            };

            // ���������� ������ �� ������
            var response = await _apiService.MakeAuthorizedRequestAsync<ApiResponse>(
                $"{_baseUrl}/api/Restaurants/reserve",
                HttpMethod.Post,
                reservationRequest
            );

            // ��������� ���������� ������
            if (response != null && response.Message == "Reservation created successfully")
            {
                await DisplayAlert("Success", "Your reservation has been successfully created.", "OK");
                var bookingPage = new MainPage(_apiService, _authService, _httpClient);
                await Navigation.PushAsync(bookingPage);
            }
            else
            {
                await DisplayAlert("Error", "Failed to create reservation. Please try again.", "OK");
            }
        }
        catch (FormatException)
        {
            await DisplayAlert("Error", "Invalid input format. Please check the entered data.", "OK");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Error", $"HTTP request failed: {ex.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }
}