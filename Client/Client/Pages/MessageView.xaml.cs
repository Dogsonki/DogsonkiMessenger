using Client.Models;
using Client.Networking;
using Client.Utility.Services;
using Client.Views;
using Xamarin.Forms;

namespace Client
{
    public partial class MessageView : ContentPage
    {
        private static MessageView _instance;

        public static void ScrollToBottom(MessageModel md)
        {
            _instance.MessageList.ScrollTo(md, ScrollToPosition.End, true);
        }

        public MessageView(string Username) //Chat with only one person
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ChatUsername.Text = $"Chatting with @{Username}";
            _instance = this;
        }

        private void MessageList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }

        //Closes chat 
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

            SocketCore.Send(" ", 7); //Close chat 

            return base.OnBackButtonPressed();
        }



        private void Entry_Completed(object sender, System.EventArgs e)
        {
            Entry input = (Entry)sender;
            string InputText = input.Text;
            if (string.IsNullOrWhiteSpace(InputText) || InputText == "ENDCHAT" || InputText == " ")
            {
                return;
            }
            InputText.Replace("$", "69420"); //Temporary replacment cuz it will break server

            MessageViewModel.AddMessage(input.Text);
            input.Text = "";
        }
    }
}