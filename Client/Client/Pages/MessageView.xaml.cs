using Client.Networking;
using Client.Views;
using Xamarin.Forms;
using Client.Utility.Services;

namespace Client
{
    public partial class MessageView : ContentPage
    {

        public MessageView() //Chat with only one person
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void MessageList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }

        protected override bool OnBackButtonPressed()
        {
            bool IsVisible = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                IKeyboardService keyboardService = DependencyService.Get<IKeyboardService>();
                IsVisible = keyboardService.IsKeyboardVisible();
            });

            if (IsVisible)
                return base.OnBackButtonPressed();

            SocketCore.SendRaw("ENDCHAT"); //Close chat 

            return base.OnBackButtonPressed();
        }

        private void Entry_Completed(object sender, System.EventArgs e)
        {
            Entry input = (Entry)sender;
            if (input.Text == "" || input.Text == "ENDCHAT") 
            {
                return;
            }
            MessageViewModel.AddMessage(input.Text);
            input.Text = "";
        }
    }
}