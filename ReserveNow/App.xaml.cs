using System.Text.Json;
using System.Timers;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ReserveNow.Views;
using Microsoft.Maui.LifecycleEvents;
using System.Net.Http;
using ReserveNow.Models;

namespace ReserveNow
{
    public partial class App : Application
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly Reservation _reservation;
        public App(AuthService authService, ApiService apiService,HttpClient httpClient, Reservation reservation)
        {
            InitializeComponent();
            _authService = authService;
            _apiService = apiService;
            _httpClient = httpClient;
            _reservation = reservation;
            var loginPage = new LoginPage(_apiService, _authService, _reservation);
            var mainPage = new MainPage(_apiService, _authService, _httpClient,_reservation);
            var (accessToken, refreshToken, expireAt) = _authService.GetSavedTokens();
            var user = _authService.GetUserProfile();

            if (expireAt<DateTime.UtcNow&&user!=null)
            {
                    MainPage = new NavigationPage(mainPage);
                    return;
            }
            else
            {
                _authService.ClearTokens();
                _authService.ClearUserProfile();
                MainPage = new NavigationPage(loginPage);
                return;

            }
            //if (!string.IsNullOrEmpty(refreshToken))
            //{
            //    // Проверяем, действителен ли refresh_token
            //    try
            //    {
            //        // Проверяем действительность refresh_token
            //        var newAccessToken = _authService.RefreshAccessTokenAsync(refreshToken);
            //        _authService.SaveTokens(newAccessToken.Result, refreshToken, expireAt);

            //        // Переходим на главную страницу

            //    }
            //    catch (Exception ex)
            //    {
            //        if (ex.Message.Contains("Refresh token expired"))
            //        {
            //            // Если refresh_token истек, очищаем токены и переходим на страницу входа
            //            _authService.ClearTokens();
            //            _authService.ClearUserProfile();
            //        }
            //    }
            //}

            //// Переходим на страницу входа
            //MainPage = new NavigationPage(loginPage);

        }
       
        //public App(ApiService apiService, AuthService authService)
        //{
        //    InitializeComponent();
        //    var httpClient = new HttpClient();
        //    _authService = authService;
        //    _apiService = apiService;


        //    // Создаем экземпляры AuthService и ApiService
        //    var loginPage = new LoginPage(apiService, authService);
        //    var mainPage = new MainPage(new ApiService(new HttpClient(), authService));

        //    // Передаем сервисы в LoginPage
        //    var (accessToken, refreshToken) = _authService.GetSavedTokens();
        //    if (!string.IsNullOrEmpty(refreshToken))
        //    {
        //        try
        //        {
        //            // Проверяем действительность refresh_token
        //            var newAccessToken = _authService.RefreshAccessTokenAsync(refreshToken);
        //            _authService.SaveTokens(Convert.ToString(newAccessToken), refreshToken);

        //            // Переходим на главную страницу
        //            _navigationPage = new NavigationPage(mainPage);
        //            MainPage = _navigationPage;
        //            return;
        //        }
        //        catch
        //        {
        //            // Если refresh_token недействителен, очищаем токены
        //            _authService.ClearTokens();
        //        }
        //    }
        //    _navigationPage = new NavigationPage(loginPage);
        //    MainPage = _navigationPage;


        //}


    }
}