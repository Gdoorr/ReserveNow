using System.Text.Json;
using System.Timers;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace ReserveNow
{
    public partial class App : Application
    {
        private System.Timers.Timer _tokenCheckTimer;
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());

        }
        

    }
}