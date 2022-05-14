using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAfterLoginPage : ContentPage
    {
        public MainAfterLoginPage()
        {
            NavigationPage.SetHasNavigationBar(this,false); 
            InitializeComponent();
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

        private void LastChatsOpened_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }
    }
}