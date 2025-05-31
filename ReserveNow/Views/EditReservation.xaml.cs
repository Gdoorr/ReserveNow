

using ReserveNow.Models;
using System.Globalization;
using System.Net.Http;
namespace ReserveNow.Views;

public partial class EditReservation : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Reservation _reservation;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly ReservationRequest _reservationRequest;
    private readonly string _baseUrl;
    public EditReservation(ReservationRequest reservationRequest, ApiService apiService,AuthService authService,HttpClient httpClient)
	{
		InitializeComponent();
        _reservationRequest = reservationRequest;
        _apiService = apiService;
        _authService = authService;
        _httpClient = httpClient;
        LoadRestaurantDetails();

        if (DateTime.TryParseExact(_reservationRequest.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            DatePicker.Date = parsedDate;
        }
        else
        {
            throw new FormatException("Invalid date format.");
        }
        StartTimePicker.SelectedItem = _reservationRequest.StartTime;
        EndTimePicker.SelectedItem = _reservationRequest.EndTime;
        GuestsEntry.Text = _reservationRequest.Guests.ToString();

        //LoadRestaurantWorkingHours(_reservationRequest.RestaurantId);
    }
    private void OnConfirmChangesClicked(object sender, EventArgs e)
    {
        // Логика сохранения изменений
    }
    private async void LoadRestaurantDetails()
    {
        try
        {
            // Получаем информацию о ресторане
            var restaurant = await _apiService.GetRestaurantAsync(_reservationRequest.RestaurantId);

            // Ограничиваем выбор времени
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
    //private async void LoadRestaurantWorkingHours(int restaurantId)
    //{
    //    try
    //    {
    //        var (openTime, closeTime) = await _apiService.GetRestaurantWorkingHoursAsync(restaurantId);

    //        StartTimePicker.MinimumTime = openTime;
    //        StartTimePicker.MaximumTime = closeTime.Subtract(TimeSpan.FromMinutes(30));

    //        EndTimePicker.MinimumTime = openTime.Add(TimeSpan.FromMinutes(30));
    //        EndTimePicker.MaximumTime = closeTime;
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", $"Failed to load working hours: {ex.Message}", "OK");
    //    }
    //}

    private void OnDateChanged(object sender, DateChangedEventArgs e)
    {
        //LoadRestaurantWorkingHours(_reservation.RestaurantId);
    }

    private void OnStartTimeChanged(object sender, EventArgs e)
    {
        // Получаем выбранные времена
        var startTimeString = StartTimePicker.SelectedItem?.ToString();
        var endTimeString = EndTimePicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
        {
            return;
        }

        var startTime = TimeSpan.Parse(startTimeString);
        var endTime = TimeSpan.Parse(endTimeString);

        // Проверяем, что StartTime меньше EndTime
        if (startTime >= endTime)
        {
            //ValidationMessage.Text = "Start time must be earlier than end time.";
            StartTimePicker.SelectedItem = null; // Очищаем выбор
        }
        else
        {
            //ValidationMessage.Text = string.Empty;
        }
    }

    private void OnEndTimeChanged(object sender, EventArgs e)
    {
        // Получаем выбранные времена
        var startTimeString = StartTimePicker.SelectedItem?.ToString();
        var endTimeString = EndTimePicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
        {
            return;
        }

        var startTime = TimeSpan.Parse(startTimeString);
        var endTime = TimeSpan.Parse(endTimeString);

        // Проверяем, что EndTime больше StartTime
        if (endTime <= startTime)
        {
            //ValidationMessage.Text = "End time must be later than start time.";
            EndTimePicker.SelectedItem = null; // Очищаем выбор
        }
        else
        {
            //ValidationMessage.Text = string.Empty;
        }
    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        try
        {
            var startTime = TimeSpan.Parse(StartTimePicker.SelectedItem.ToString());
            var endTime = TimeSpan.Parse(EndTimePicker.SelectedItem.ToString());
            var guests = int.Parse(GuestsEntry.Text);

            if (startTime >= endTime)
            {
                await DisplayAlert("Ошибка", "Время начала должно быть раньше времени окончания.", "Ок");
                return;
            }

            if (guests <= 0)
            {
                await DisplayAlert("Ошибка", "Количество гостей должно быть больше нуля.", "Ок");
                return;
            }

            var date = DatePicker.Date.ToString("yyyy-MM-dd");

            // 3. Проверяем доступность столов через API
            var availableTables = await _apiService.GetAvailableTablesAsync(
                _reservationRequest.RestaurantId,
                DateTime.Parse(date),
                startTime,
                endTime,
                guests
            );

            //if (!availableTables.Any())
            //{
            //    await DisplayAlert("Error", "No available tables for the specified time and number of guests.", "OK");
            //    return;
            //}

            //// 4. Выбираем ID первого доступного стола
            //var firstAvailableTableId = availableTables.First().ID;

            // 5. Обновляем бронь с новым ID стола
            await _apiService.UpdateReservationAsync(_reservationRequest.ID, new Reservation
            {
                ID = _reservationRequest.ID,
                UserId = _reservationRequest.UserId,
                RestaurantId = _reservationRequest.RestaurantId,
                RestaurantName = _reservationRequest.RestaurantName,
                RestaurantCity =_reservationRequest.RestaurantCity,
                TableId = availableTables.ID, // Используем ID первого доступного стола
                Date = date,
                StartTime = startTime.ToString(@"hh\:mm\:ss"),
                EndTime = endTime.ToString(@"hh\:mm\:ss"),
                Guests = guests
            });

            // 6. Уведомляем пользователя об успешном обновлении
            await DisplayAlert("Успех", "Бронирование успешно обновлено.", "Ок");
            var bookingPage = new MainPage(_apiService, _authService, _httpClient, _reservation);
            await Navigation.PushAsync(bookingPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update reservation: {ex.Message}", "OK");
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
        var editPage = new AccountSettingsPage(_apiService, _authService, _httpClient, _reservation);
        Navigation.PushAsync(editPage);
    }

}