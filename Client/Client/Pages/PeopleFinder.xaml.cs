using Client.Networking;
using Client.Views;
using Newtonsoft.Json.Linq;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PeopleFinder : ContentPage
    {
        public PeopleFinder()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            PeopleFinderViewModel.PeopleFound.Clear();
            InitializeComponent();
        }

        private void FindClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameFind.Text))
                return;

            Console.WriteLine($"Looking for people ... {UsernameFind.Text}");

            PeopleFinderViewModel.ClearList();
            SocketCore.SendR(ParseQuery, $"{UsernameFind.Text}", 4);
        }

        public static void ParseQuery(object req)
        {
            JArray users;
            try
            {
                users = (JArray)req;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot parse usernames: {req} : " + ex);
                return;
            }

            foreach (var a in users)
            {
                PeopleFinderViewModel.AddFound(a.ToString());
            }
        }

        private void PeopleFound_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }
    }
}