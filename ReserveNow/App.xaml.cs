using System.Text.Json;
using System.Timers;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace ReserveNow
{
    public partial class App : Application
    {
        private System.Timers.Timer _tokenCheckTimer;
        public App()
        {
            InitializeComponent();
            SetupTokenCheck();

        }
        private void SetupTokenCheck()
        {
            // Проверка токена при старте
            CheckTokenAndNavigate();

            // Запускаем фоновую проверку каждые 5 минут (300000 мс)
            _tokenCheckTimer = new System.Timers.Timer(300000);
            _tokenCheckTimer.Elapsed += OnTokenCheck;
            _tokenCheckTimer.AutoReset = true;
            _tokenCheckTimer.Start();
        }

        private async void CheckTokenAndNavigate()
        {
            var token = await SecureStorage.GetAsync("AccessToken");
            if (string.IsNullOrEmpty(token) || JwtHelper.IsTokenExpired(token))
            {
                MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                MainPage = new NavigationPage(new MainPage());
            }
        }

        private async void OnTokenCheck(object sender, ElapsedEventArgs e)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var token = await SecureStorage.GetAsync("AccessToken");
                if (JwtHelper.IsTokenExpired(token))
                {
                    // Токен истек: перезагружаем приложение на страницу входа
                    MainPage = new NavigationPage(new LoginPage());
                }
            });
        }

    }
}