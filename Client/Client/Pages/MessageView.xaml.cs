using Client.Networking;
using Client.Views;
using Xamarin.Forms;
using Client.Utility.Services;

namespace Client
{
    public partial class MessageView : ContentPage
    {

        public MessageView(string Username) //Chat with only one person
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ChatUsername.Text = $"Chatting with @{Username}";
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

            SocketCore.SendRaw(" ",7); //Close chat 

            return base.OnBackButtonPressed();
        }

        private void Entry_Completed(object sender, System.EventArgs e)
        {
            Entry input = (Entry)sender;
            string InputText = input.Text;
            if (InputText == "" || InputText == "ENDCHAT") 
            {
                return;
            }
            InputText.Replace("$", "69420"); //Temporary replacment cuz it will break server

            MessageViewModel.AddMessage(input.Text);
            input.Text = "";
        }
    }
}