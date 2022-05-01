using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DogsonkiMessenger.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppEntry : ContentPage
    {
        public AppEntry()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        private void LoginOptionClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LoginView());
        }

        private void RegisterOptionClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new RegisterView());
        }
    }
}