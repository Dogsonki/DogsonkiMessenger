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
        }

        private void LoginDone(object sender, EventArgs e)
        {
            string username = Input_Username.Text;
            string password = Input_Password.Text;
            MainUser.Username = username;

            SocketCore.SendRaw("logging");
            SocketCore.SendRaw(username);
            SocketCore.SendR(LoginCallback, password, 0001, 0001);
        }

        private void LoginCallback(string msg)
        {
            Navigation.PopAsync();
            Navigation.PushAsync(new MessageView());
        }
    }
}