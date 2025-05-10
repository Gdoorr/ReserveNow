using System.Text.Json;
using System.Timers;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ReserveNow.Views;
using Microsoft.Maui.LifecycleEvents;
using System.Net.Http;

namespace ReserveNow
{
    public partial class App : Application
    {
        //private readonly ApiService _apiService;
        //private readonly AuthService _authService;
        public App(AuthService authService, ApiService apiService,HttpClient httpClient)
        {
            InitializeComponent();

            // Устанавливаем временную стартовую страницу
            MainPage = new NavigationPage(loginPage);


            // Передаем сервисы в LoginPage
            //MainPage = new NavigationPage(new LoginPage(apiService, authService));
            InitializeAppAsync(authService, apiService,httpClient).GetAwaiter().GetResult();

        }
        private async Task InitializeAppAsync(AuthService authService, ApiService apiService,HttpClient httpClient)
        {
            var loginPage = new LoginPage(apiService, authService);
            var mainPage = new MainPage(apiService, authService,httpClient);
            var (accessToken, refreshToken) = authService.GetSavedTokens();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    // Проверяем действительность refresh_token
                    var newAccessToken = await authService.RefreshAccessTokenAsync(refreshToken);

                    authService.SaveTokens(Convert.ToString(newAccessToken), refreshToken);
                    var tokens = authService.GetSavedTokens();
                    if (!string.IsNullOrEmpty(tokens.AccessToken) || !string.IsNullOrEmpty(tokens.RefreshToken))
                    {
                        MainPage = new NavigationPage(mainPage);
                        return;
                    }
                    else
                    {
                        MainPage = new NavigationPage(loginPage);
                        return;
                    }
                    // Переходим на главную страницу

                }
                catch
                {
                    // Если refresh_token недействителен, очищаем токены
                    authService.ClearTokens();
                    authService.ClearUserProfile();
                    MainPage = new NavigationPage(loginPage);
                    return;
                }
            }
            MainPage = new NavigationPage(loginPage);
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