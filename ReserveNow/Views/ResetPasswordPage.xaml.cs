using System.Net.Http.Json;
namespace ReserveNow.Views;

public partial class ResetPasswordPage : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private readonly string _baseUrl;
    public ResetPasswordPage()
	{
        InitializeComponent();
        _baseUrl = MauiProgram.Configuration["ServerSettings:BaseUrl"];
    }
    private async void OnSendCodeClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;

        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/ResetPassword/forgot-password", new { Email = email });

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "Код отправлен на ваш email.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Не удалось отправить код.", "OK");
        }
    }
    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var code = CodeEntry.Text;
        var newPassword = NewPasswordEntry.Text;

        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/ResetPassword/reset-password", new
        {
            Email = email,
            ResetCode = code,
            NewPassword = newPassword
        });

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "Пароль успешно обновлен.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Неверный код или ошибка обновления пароля.", "OK");
        }
    }
}