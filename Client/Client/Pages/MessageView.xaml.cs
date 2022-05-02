using Client.Views;
using Xamarin.Forms;

namespace Client
{
    public partial class MessageView : ContentPage
    {

        public MessageView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        private void MessageList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }

        private void Entry_Completed(object sender, System.EventArgs e)
        {
            MessageViewModel.AddMessage(((Entry)sender).Text, "Test");
        }
    }
}