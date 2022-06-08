using Client.Models;
using Client.Networking;
using Client.Pages;
using Client.Utility.Services;
using Client.Views;
using Xamarin.Forms;

namespace Client
{
    public partial class MessagePage : ContentPage
    {
        private static MessagePage _instance;

        public static void ScrollToBottom(MessageModel md)
        {
            _instance.MessageList.ScrollTo(md, ScrollToPosition.End, true);
        }

        public MessagePage(string Username) //Chat with only one person
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ChatUsername.Text = $"Chatting with @{Username}";
            _instance = this;
        }

        private void MessageListSelected(object sender, SelectedItemChangedEventArgs e)
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

            SocketCore.Send(" ", Token.END_CHAT); //Close chat 
            MessagePageView.ClearMessages();
            MainAfterLoginPage.RedirectToInstace();
            return true;    
        }



        private void MessageCompleted(object sender, System.EventArgs e)
        {
            Entry input = (Entry)sender;
            string InputText = input.Text;
            if (string.IsNullOrWhiteSpace(InputText) || InputText == "ENDCHAT" || InputText == " ")
            {
                return;
            }
            InputText.Replace("$", "69420"); //Temporary replacment cuz it will break server

            MessagePageView.AddMessage(input.Text);
            input.Text = "";
        }
    }
}