using System.Net.Http.Json;
using ReserveNow.Models;

namespace ReserveNow.Views;

public partial class RegisterPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public RegisterPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        LoadCities();
    }

    private async void LoadCities()
    {
        // Загрузка списка городов с API (если есть)
        var cities = await _httpClient.GetFromJsonAsync<List<City>>("http://localhost:5000/api/Registration/cities");
        foreach (var city in cities)
        {
            CityPicker.Items.Add(city.Name);
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var selectedCityIndex = CityPicker.SelectedIndex;
        if (selectedCityIndex == -1)
        {
            await DisplayAlert("Error", "Please select a city", "OK");
            return;
        }

        var userDto = new RegistrationUserDto
        {
            Name = NameEntry.Text,
            Email = EmailEntry.Text,
            Phone = PhoneEntry.Text,
            Password = PasswordEntry.Text,
            CityId = selectedCityIndex + 1 // Предполагаем, что ID города соответствует индексу + 1
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/api/Registration/register", userDto);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "User registered successfully", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", error, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}