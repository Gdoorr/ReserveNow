using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReserveNow.Models;
using ReserveNow.Services;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace ReserveNow.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                var loginRequest = new LoginRequest
                {
                    Username = Username,
                    Password = Password
                };

                var loginResponse = await _authService.LoginAsync(loginRequest);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    // Сохраняем токен в SecureStorage
                    await SecureStorage.SetAsync("AccessToken", loginResponse.Token);

                    // Переходим на главную страницу приложения
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Ошибка", "Неверные учетные данные", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
