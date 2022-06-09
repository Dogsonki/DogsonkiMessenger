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
        public AppEntry(bool readSession)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            SocketCore.Init();

            if (readSession)
            {
                ReadStorage();
            }
        }

        private void ReadStorage()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                var session = StorageIO.ReadStorage<Session>("session", new Session("", ""));

                if (!string.IsNullOrEmpty(session.SessionKey))
                {
                    SocketCore.Send(session, Token.SESSION_INFO);
                }
            }
        }

        private void LoginOptionClicked(object sender, EventArgs e) => Navigation.PushAsync(new LoginPage());

        private void RegisterOptionClicked(object sender, EventArgs e) => Navigation.PushAsync(new RegisterPage());   
    }
}