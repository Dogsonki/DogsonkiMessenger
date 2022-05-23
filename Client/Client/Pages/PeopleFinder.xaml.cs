using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Client.Networking;
using System;
using Newtonsoft.Json.Linq;
using Client.Views;

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
            LocalUser.ActualChatWith = UsernameFind.Text;
            SocketCore.SendR(ParseQuery, $"1-{UsernameFind.Text}", 0004);
        }

        public static void ParseQuery(string req)
        {
            JArray users;
            try
            {
                 users= JArray.Parse(req);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot parse usernames: {req} : "+ex);
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