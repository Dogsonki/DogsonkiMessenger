using Client.Models;
using Client.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAfterLoginPage : ContentPage
    {
        public static MainAfterLoginPage Instance { get; set; }

        public MainAfterLoginPage(bool redirectedFormLogin = false)
        {
            if (Instance == null)
                Instance = this;
            
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            if (redirectedFormLogin)
                MainAfterLoginPageView.Clear();
        }
        private void FindPeople_Clicked(object sender, System.EventArgs e) => Navigation.PushAsync(new SearchPage()); //Don't pop cuz you can get back to "main menu"

        public static void RedirectToInstace() => StaticNavigator.PopAndPush(Instance);

        protected override bool OnBackButtonPressed()
        {
            LocalUser.Logout();
            return true;
        }

        private void LastChatsOpenedSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }

        private void SearchPressed(object sender, EventArgs e) => SearchPage.RedirectAndSearch(SearchInput.Text);

        private void SettingsTapped(object sender, EventArgs e) => Navigation.PushAsync(new ProfileOptions());
    }
}