using Client.IO;
using Client.Model.Session;
using Client.Networking;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppEntry : ContentPage
    {
        public AppEntry()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            SocketCore.Init();
            ReadStorage();
        }

        private void ReadStorage()
        {
            //Read cache to auto login 
            //Need to do token 
            if (Device.RuntimePlatform == Device.Android)
            {
                var r = StorageIO.ReadStorage<Session>("session", new Session("", ""));

                if (!string.IsNullOrEmpty(r.SessionKey))
                {
                    SocketCore.SendRaw(r, 9);
                }
            }
        }

        private void LoginOptionClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
            Navigation.PushAsync(new LoginView());
        }

        private void RegisterOptionClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
            Navigation.PushAsync(new RegisterView());
        }
    }
}