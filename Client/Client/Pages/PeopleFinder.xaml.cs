using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Client.Networking;
using System;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PeopleFinder : ContentPage
    {
        public PeopleFinder()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

        }

        private void PeopleFindCallback(string rev)
        {
            Console.WriteLine("Callback::"+rev);
        }

        private void FindClicked(object sender, EventArgs e)
        {
            SocketCore.SendR(PeopleFindCallback, $"1-{UsernameFind.Text}", null);
        }
    }
}