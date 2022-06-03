using Client.Models;
using Client.Networking;
using Client.Pages;
using Client.Utility;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        protected char[] IllegalCharacters = { '$', '*', '{', '}', '#', '@' };

        public RegisterPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

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

            if(Input_RepeatPassword.Text != Input_Password.Text)
            {
                ShowError("Make sure your passwords match");
                return;
            }

            if ((IllegalCharIndex = username.IndexOfAny(IllegalCharacters)) > 0)
            {
                ShowError($"Username contains illegal character - {username[IllegalCharIndex]}");
                return;
            }
            if(!SocketCore.SendR(RegisterCallback, new RegisterModel(username,password), Token.REGISTER))
            {
                ShowError("Unable to connect to the server");
                return;
            }
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

        private void RegisterCallback(object rev)
        {
            string recived = (string)rev;
            if (recived[0] == '1')
            {
                //Check if this function have to be invoked in main thread
                Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); await Navigation.PushAsync(new LoginPage()); });
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

        private void ToLogin_Tapped(object sender, EventArgs e)
        {
            StaticNavigator.PopAndPush(new LoginPage());
        }
    }
}