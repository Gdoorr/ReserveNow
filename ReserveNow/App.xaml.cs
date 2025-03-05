namespace ReserveNow
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            CheckTokenAndNavigate();
        }

        private async void CheckTokenAndNavigate()
        {
            var token = await SecureStorage.GetAsync("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                MainPage = new LoginPage();
            }
        }
    }
}