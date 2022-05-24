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
            ReadCache();
        }

        private void ReadCache()
        {
            //Read cache to auto login 
            //Need to do token 
            if(Device.RuntimePlatform == Device.Android)
            {

            }
            else
            {

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