using Client.Networking;
using Client.Views;
using Newtonsoft.Json.Linq;
using System.Collections;
using Xamarin.Forms;

namespace Client
{
    public partial class MessageView : ContentPage
    {

        public MessageView() //Chat with only one person
        {
            NavigationPage.SetHasNavigationBar(this, false);

            InitializeComponent();
        }

        public MessageView(IEnumerable ChatWith) //Initialize group chat 
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