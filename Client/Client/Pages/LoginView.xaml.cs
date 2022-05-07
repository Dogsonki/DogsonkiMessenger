using Client.Networking;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginView : ContentPage
    {
        public LoginView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
#if LOGINAUTOTEST
            SocketCore.SendRaw("logging");
            SocketCore.SendRaw("aaa");
            SocketCore.SendR(LoginCallback, "aaa", 0001, 0001);
#endif
        }

        private void LoginDone(object sender, EventArgs e)
        {
            string username = Input_Username.Text;
            string password = Input_Password.Text;
            MainUser.Username = username;

            SocketCore.SendRaw("logging");
            SocketCore.SendRaw(username);
            SocketCore.SendR(LoginCallback, password, 0001, 0001);
            //1 == logged 
            //0 == samething wrong 
        }


        private void LoginCallback(string rev)
        {
            switch (rev[0])
            {
                case '1':
                    Console.WriteLine("Logging....");
                    Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new PeopleFinder()); });
                    break;
                case '0':
                    Console.WriteLine("Password is wrong");
                    break;
                default:
                    Console.WriteLine("Samething went wrong: " + rev);
                    break;
            }
        }
    }
}