using Client.Networking;
using Client.Utility;
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
        }

        private void LoginDone(object sender, EventArgs e)
        {
            string username = Input_Username.Text;
            string password = Input_Password.Text;
            LocalUser.Username = username;
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
                    StaticNavigator.Push(new MainAfterLoginPage());
                    LocalUser.Username = Input_Username.Text;
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