
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
        // �������� ������ ������������
        LoadUserData();
        
    }
    private async void LoadUserData()
    {
        try
        {
            // �������� �������� ������������ �� ���������� ���������
            var currentUser = _authService.GetUserProfile();

            if (currentUser == null)
            {
                await DisplayAlert("������", "������� ������������ �� ������.", "OK");
                return;
            }

            // ��������� ���� ������� ������������
            NameEntry.Text = currentUser.Name;
            EmailEntry.Text = currentUser.Email;
            PhoneEntry.Text = currentUser.Phone;

            // ��������� �������� �������
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
            // ���������� ��������� ���� ��� ����� ������
            var passwordPromptPage = new PasswordPromptPage();
            await Navigation.PushModalAsync(passwordPromptPage);

            // ����, ���� ������������ ������� ����
            while (passwordPromptPage.EnteredPassword == null && passwordPromptPage.Navigation.ModalStack.Count > 0)
            {
                await Task.Delay(100); // ��������� �������� ��� �������� ����������
            }

          var currentPassword = passwordPromptPage.EnteredPassword;

            if (string.IsNullOrEmpty(currentPassword))
            {
                await DisplayAlert("������", "��������� ������� ������.", "��");
                return;
            }

            // ���������, ��� ��������� ������ ��������� � �����������
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, currentUser.Password))
            {
                await DisplayAlert("������", "�������� ������� ������.", "��");
                return;
            }

            // �������� ����� ������ �� �����
           var updatedClient = new Client
            {
                ID = currentUser.ID,
                Name = NameEntry.Text?.Trim(),
                Email = EmailEntry.Text?.Trim(),
                Phone = PhoneEntry.Text?.Trim(),
                Password = string.IsNullOrEmpty(PasswordEntry.Text) ? null : PasswordEntry.Text?.Trim(),
                CityId = CityPicker.SelectedIndex// ��������������, ��� CityId ���������� � 1
            };

            // ��������� ���������� ������
            if (string.IsNullOrEmpty(updatedClient.Name))
            {
                await DisplayAlert("������", "��������� ������� ���.", "��");
                return;
            }

            if (string.IsNullOrEmpty(updatedClient.Email) || !IsValidEmail(updatedClient.Email))
            {
                await DisplayAlert("������", "�������� ����� ����������� �����.", "��");
                return;
            }

            // ���������� ����������� ������ �� ������
            await _apiService.UpdateUserAsync(updatedClient);

            // ��������� ����������� ������ � ��������� ���������
            _authService.SaveUserProfile(updatedClient);

            // ���������� ������������ �� �������� ����������
            await DisplayAlert("�����", "������� ������� ��������.", "��");
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
        // ������� ������ ������������ (���� ����������)
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