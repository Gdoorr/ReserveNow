
using ReserveNow.Models;
using System.Text.Json;
namespace ReserveNow.Views;

public partial class AccountSettingsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly Reservation _reservation;
    private Client _currentUser;
    public AccountSettingsPage(ApiService apiService, AuthService authService, HttpClient httpClient, Reservation reservation)
    {
        InitializeComponent();
        _apiService = apiService;
        _authService = authService;
        _httpClient = httpClient;
        _reservation = reservation;
        // Загрузка данных пользователя
        LoadUserData();
        
    }
    private async void LoadUserData()
    {
        try
        {
            // Получаем текущего пользователя из локального хранилища
            var currentUser = _authService.GetUserProfile();

            if (currentUser == null)
            {
                await DisplayAlert("Ошибка", "Профиль пользователя не найден.", "OK");
                return;
            }

            // Заполняем поля данными пользователя
            NameEntry.Text = currentUser.Name;
            EmailEntry.Text = currentUser.Email;
            PhoneEntry.Text = currentUser.Phone;

            // Заполняем селектор городов
            var cities = await _apiService.GetCitiesAsync();
            CityPicker.ItemsSource = cities;
            CityPicker.SelectedItem = currentUser.City;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load user data: {ex.Message}", "OK");
        }
    }
    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        try
        {
            var currentUser = _authService.GetUserProfile();
            // Показываем модальное окно для ввода пароля
            var passwordPromptPage = new PasswordPromptPage();
            await Navigation.PushModalAsync(passwordPromptPage);

            // Ждем, пока пользователь закроет окно
            while (passwordPromptPage.EnteredPassword == null && passwordPromptPage.Navigation.ModalStack.Count > 0)
            {
                await Task.Delay(100); // Небольшая задержка для ожидания результата
            }

          var currentPassword = passwordPromptPage.EnteredPassword;

            if (string.IsNullOrEmpty(currentPassword))
            {
                await DisplayAlert("Ошибка", "Требуется текущий пароль.", "Ок");
                return;
            }

            // Проверяем, что введенный пароль совпадает с сохраненным
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, currentUser.Password))
            {
                await DisplayAlert("Ошибка", "Неверный текущий пароль.", "Ок");
                return;
            }

            // Получаем новые данные из полей
           var updatedClient = new Client
            {
                ID = currentUser.ID,
                Name = NameEntry.Text?.Trim(),
                Email = EmailEntry.Text?.Trim(),
                Phone = PhoneEntry.Text?.Trim(),
                Password = string.IsNullOrEmpty(PasswordEntry.Text) ? null : PasswordEntry.Text?.Trim(),
                CityId = CityPicker.SelectedIndex// Предполагается, что CityId начинается с 1
            };

            // Проверяем валидность данных
            if (string.IsNullOrEmpty(updatedClient.Name))
            {
                await DisplayAlert("Ошибка", "Требуется указать имя.", "Ок");
                return;
            }

            if (string.IsNullOrEmpty(updatedClient.Email) || !IsValidEmail(updatedClient.Email))
            {
                await DisplayAlert("Ошибка", "Неверный адрес электронной почты.", "Ок");
                return;
            }

            // Отправляем обновленные данные на сервер
            await _apiService.UpdateUserAsync(updatedClient);

            // Сохраняем обновленные данные в локальное хранилище
            _authService.SaveUserProfile(updatedClient);

            // Уведомляем пользователя об успешном обновлении
            await DisplayAlert("Успех", "Профиль успешно обновлен.", "Ок");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update profile: {ex.Message}", "OK");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Очистка данных пользователя (если необходимо)
        var editPage = new LoginPage(_apiService, _authService, _reservation);
        Navigation.PushAsync(editPage);
        NavigationPage.SetHasNavigationBar(this, false);

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
        var editPage = new AccountSettingsPage(_apiService, _authService,_httpClient,_reservation);
        Navigation.PushAsync(editPage);
    }
}