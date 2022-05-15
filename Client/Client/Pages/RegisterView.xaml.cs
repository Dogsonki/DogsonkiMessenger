using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Client.Networking;
using Client.Pages;
using System.Text;
using System.Linq;

namespace Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterView : ContentPage
    {
        public RegisterView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        protected char[] IllegalCharacters = { '*', '{', '}', '#', '@' };

        private void RegisterDone(object sender, EventArgs e)
        {
            string username = Input_Username.Text;
            string password = Input_Password.Text;

            int IllegalCharIndex;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username or password is empty");
                return;
            }

            if ((IllegalCharIndex = username.IndexOfAny(IllegalCharacters)) > 0)
            {
                ShowError($"Username contains illegal character - {username[IllegalCharIndex]}");
                return;
            }

            if (!SocketCore.SendRaw("registering"))
            {
                ShowError("Samething went wrong, probably on server side ... ");
            }

            SocketCore.SendRaw(username);
            SocketCore.SendR(RegisterCallback, password,0002);
        }

        protected Label _ErrorText = new Label();
        protected bool _AlreadyShowed = false;

        private void ClearError()
        {
            _ErrorText.Text = "";
            ErrorLevel.Children.Remove(_ErrorText);
            _AlreadyShowed = false;
        }

        private void ShowError(string text)
        {
            if (!_AlreadyShowed)
            {
                _ErrorText.TextColor = Color.Red;
                ErrorLevel.Children.Add(_ErrorText);
                _AlreadyShowed = true;
            }
            _ErrorText.Text = text;
        }

        private void RegisterCallback(string rev)
        {
            string token = rev.Substring(0, 2);
            if (token.ToString() == "1")
            {
                //Check if this function have to be invoked in main thread
                Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); await Navigation.PushAsync(new LoginView()); });
            }
            else
            {
                ShowError("This username is already registred");
            }
        }

        private void Input_Focused(object sender, FocusEventArgs e)
        {
            ClearError();
        }
    }
}