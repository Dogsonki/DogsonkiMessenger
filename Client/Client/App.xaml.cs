using Xamarin.Forms;
using Client.Pages;
using Client.Networking;

namespace Client
{
    public partial class App : Application
    {
        public static App app; //It's instance
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new AppEntry());
            SocketCore.Init();
        }

        public static void ChangePage(Page page) => app.MainPage = page;

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
