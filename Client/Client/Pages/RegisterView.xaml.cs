using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Client.Networking;
using Client.Pages;
using System.Text;

namespace Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterView : ContentPage
    {
        public RegisterView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        private void RegisterDone(object sender, EventArgs e)
        {
            string username = Input_Username.Text;
            string password = Input_Password.Text;

            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return;
            }

            SocketCore.SendRaw("registering");
            SocketCore.SendRaw(username);
            SocketCore.SendR(RegisterCallback, password,0002,0002);
        }

        private void RegisterCallback(string rev)
        {
            StringBuilder token = new StringBuilder();
            token.Append(rev[0]);
            token.Append(rev[1]);
            if (token.ToString() == "01")
            {
                Console.WriteLine("Registred");//Check if this function have to be invoked in main thread
                Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); await Navigation.PushAsync(new LoginView()); });
            }
            else
            {
                //Samething went wrong !

                //01 => already registred
            }
        }
    }
}