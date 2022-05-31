using Xamarin.Forms;
using Client.Pages;
using Client.Utility;

namespace Client
{
    public partial class App : Application
    {
        public App()
        {
            Debug.Write("Entry");
            InitializeComponent();
            MainPage = new NavigationPage(new AppEntry());
        }
    }
}
