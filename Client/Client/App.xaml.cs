using Xamarin.Forms;
using Client.Pages;
using Client.Networking;

namespace Client
{
    public partial class App : Application
    {
        public static App app; 

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new AppEntry());
            SocketCore.Init();
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {

        }
    }
}
