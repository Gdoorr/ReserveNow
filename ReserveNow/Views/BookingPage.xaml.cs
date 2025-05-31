using ReserveNow.Models;
using Microsoft.Maui.Controls;
namespace ReserveNow.Views;

public partial class BookingPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Restaurant _restaurant;
    private readonly AuthService _authService;
    private readonly Reservation _reservation;
    private readonly HttpClient _httpClient;
    private Table _selectedTable;
    private readonly string _baseUrl;
    public BookingPage(ApiService apiService, Restaurant restaurant, AuthService authService,HttpClient httpClient, Reservation reservation)
    {
        InitializeComponent();

        _apiService = apiService;
        _restaurant = restaurant;
        _authService = authService;
        _httpClient = httpClient;
        _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
        LoadRestaurantDetails();
        _reservation = reservation;
    }
    private async void LoadRestaurantDetails()
    {
        try
        {
            // Получаем информацию о ресторане
            var restaurant = await _apiService.GetRestaurantAsync(_restaurant.ID);

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
    private async void OnCheckAvailabilityClicked(object sender, EventArgs e)
    {
        try
        {
            var date = DatePicker.Date;

            // Получаем выбранное время из кастомного TimePicker
            var startTimeString = StartTimePicker.SelectedItem?.ToString();
            var endTimeString = EndTimePicker.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
            {
                await DisplayAlert("Ошибка", "Пожалуйста, выберите время начала и окончания.", "Ок");
                return;
            }

            var startTime = TimeSpan.Parse(startTimeString);
            var endTime = TimeSpan.Parse(endTimeString);

            var guests = int.Parse(GuestsEntry.Text);

            var availableTables = await _apiService.GetAvailableTablesAsync(
                _restaurant.ID, date, startTime, endTime, guests);

            if (availableTables == null)
            {
                // Нет доступных столов
                AvailabilityStatus.Text = "Нет свободных столов на данный промежуток времени.";
                AvailabilityStatus.TextColor = Colors.Red;
                AvailabilityStatus.IsVisible = true;
                ConfirmBookingButton.IsVisible = false; // Скрываем кнопку подтверждения
            }
            else
            {
                // Найден доступный столик
                _selectedTable = availableTables;
                AvailabilityStatus.Text = $"Найдены свободные столики: {_selectedTable.Capacity} .";
                AvailabilityStatus.TextColor = Colors.Green;
                AvailabilityStatus.IsVisible = true;
                ConfirmBookingButton.IsVisible = true; // Показываем кнопку подтверждения
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    //private void OnConfirmChangesClicked(object sender, EventArgs e)
    //{
    //    // Получаем выбранное время из StartTimePicker
    //    var startTimeString = StartTimePicker.SelectedItem?.ToString();
    //    if (string.IsNullOrEmpty(startTimeString))
    //    {
    //        return;
    //    }

    //    var startTime = TimeSpan.Parse(startTimeString);

    //    // Устанавливаем минимальное время для EndTimePicker
    //    EndTimePicker.MinimumTime = startTime.Add(TimeSpan.FromMinutes(30));
    //    EndTimePicker.MaximumTime = EndTimePicker.MaximumTime; // Оставляем максимальное время без изменений

    //    // Проверяем, что EndTimePicker не содержит недопустимое время
    //    var endTimeString = EndTimePicker.SelectedItem?.ToString();
    //    if (!string.IsNullOrEmpty(endTimeString))
    //    {
    //        var endTime = TimeSpan.Parse(endTimeString);
    //        if (endTime <= startTime)
    //        {
    //            ValidationMessage.Text = "End time must be later than start time.";
    //            EndTimePicker.SelectedItem = null; // Очищаем выбор
    //        }
    //        else
    //        {
    //            ValidationMessage.Text = string.Empty;
    //        }
    //    }
    //}
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
            ValidationMessage.Text = "Время начала должно быть раньше времени окончания.";
            StartTimePicker.SelectedItem = null; // Очищаем выбор
        }
        else
        {
            ValidationMessage.Text = string.Empty;
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
            ValidationMessage.Text = "Время окончания должно быть позже времени начала.";
            EndTimePicker.SelectedItem = null; // Очищаем выбор
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

            // Проверяем, выбраны ли начальное и конечное время
            if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
            {
                await DisplayAlert("Ошибка", "Пожалуйста, выберите время начала и окончания.", "Ок");
                return;
            }

            // Преобразуем дату в UTC и формат ISO 8601
            var dateUtc = DatePicker.Date.ToUniversalTime(); // Преобразуем в UTC
            var dateIso8601 = dateUtc.ToString("yyyy-MM-dd"); // Формат ISO 8601

            // Проверяем количество гостей
            if (!int.TryParse(GuestsEntry.Text, out var guests) || guests <= 0)
            {
                await DisplayAlert("Ошибка", "Пожалуйста, укажите действительное количество гостей.", "Ок");
                return;
            }

            // Формируем объект запроса
            var reservationRequest = new Reservation
            {
                UserId = userProfile.ID, // ID пользователя
                RestaurantId = _restaurant.ID,
                RestaurantName = _restaurant.Name, // Имя ресторана
                RestaurantCity = _restaurant.City,
                TableId = _selectedTable.ID,
                Date = dateIso8601, // Дата в формате ISO 8601 с временем UTC
                StartTime = startTimeString + ":00", // Время в формате строки hh:mm:ss
                EndTime = endTimeString + ":00",    // Время в формате строки hh:mm:ss
                Guests = guests
            };

            // Отправляем запрос на сервер
            var response = await _apiService.MakeAuthorizedRequestAsync<ApiResponse>(
                $"{_baseUrl}/api/Restaurants/reserve",
                HttpMethod.Post,
                reservationRequest
            );

            // Проверяем успешность ответа
            if (response != null && response.Message == "Reservation created successfully")
            {
                await DisplayAlert("Успех", "Ваше бронирование было успешно создано.", "Ок");
                var bookingPage = new MainPage(_apiService, _authService, _httpClient,_reservation);
                await Navigation.PushAsync(bookingPage);
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось создать резервирование. Пожалуйста, попробуйте снова.", "Ок");
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