using Xamarin.Forms;
using Client.Pages;

namespace Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new AppEntry(true));
        }
    }
}
