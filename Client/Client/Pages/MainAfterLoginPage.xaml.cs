#define l
using Client.Networking;
using Client.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using System.Reflection;
using System;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAfterLoginPage : ContentPage
    {
        public static Image h;
        public static MainAfterLoginPage MainInstance { get; set; }

        public MainAfterLoginPage(bool redirectedFormLogin = false)
        {
            NavigationPage.SetHasNavigationBar(this,false); 
            InitializeComponent();
            MainInstance = this;
            h = here;
                if (redirectedFormLogin)
                    MainAfterLoginViewModel.Clear();
        }

        private void FindPeople_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new PeopleFinder()); //Don't pop cuz you can get back to "main menu"
        }

        private void FriendList_Clicked(object sender, System.EventArgs e)
        {
            return;
            Navigation.PushAsync(null); //TODO: Friend list ....
        }

        protected override bool OnBackButtonPressed()
        {
            SocketCore.SendRaw("2");
            LogOut("");
            return base.OnBackButtonPressed();
        }

        private void LogOut(string rev)
        {
            LocalUser.IsLoggedIn = false;
            LocalUser.ActualChatWith = "";
            LocalUser.Username = "";
        }

        private void LastChatsOpened_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }
    }
}