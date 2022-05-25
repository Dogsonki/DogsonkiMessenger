using Client.Pages;
using Xamarin.Forms;

namespace Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new AppEntry());
        }
    }
}
