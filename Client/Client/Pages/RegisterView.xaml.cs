using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Client.Networking;
    
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
            MainUser.Username = username;
            SocketCore.SendRaw("registering");
            SocketCore.SendRaw(username);
            SocketCore.SendR(RegisterCallback, password,0002,0002);
        }

        private void RegisterCallback(string d)
        {
            Console.WriteLine("Changing page?");

            Navigation.PopAsync();
            Navigation.PushAsync(new MessageView());
        }
    }
}