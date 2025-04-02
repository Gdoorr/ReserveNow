namespace ReserveNow
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService(new HttpClient(), new AuthService(new HttpClient()));
        }

        private async void OnFetchDataClicked(object sender, EventArgs e)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://yourapi.com/data");
                var response = await _apiService.SendRequestAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    DataLabel.Text = $"Data: {data}";
                }
                else
                {
                    DataLabel.Text = $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                DataLabel.Text = $"Exception: {ex.Message}";
            }
        }
    }

}
