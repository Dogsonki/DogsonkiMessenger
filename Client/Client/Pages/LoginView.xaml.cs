using Client.Models;
using Client.Networking;
using Client.Utility;
using Client.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginView : ContentPage
    {
        public LoginView()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        private void LoginDone(object sender, EventArgs e)
        {
            string username = Input_Username.Text;
            string password = Input_Password.Text;

            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username or password is empty");
                return;
            }    

            if(!SocketCore.SendR(LoginCallback, new LoginModel(username,password,CheckRemember.IsChecked), 1))
            {
                ShowError("Unable to connect");
                return;
            }
        }

        protected Label _ErrorText = new Label();
        protected bool _AlreadyShowen = false;  

        private void ClearError()
        {
            _ErrorText.Text = "";
            ErrorLevel.Children.Remove(_ErrorText);
            _AlreadyShowen = false;
        }

        private void ShowError(string text)
        {
            if (!_AlreadyShowen)
            {
                _ErrorText.TextColor = Color.Red;
                ErrorLevel.Children.Add(_ErrorText);
                _AlreadyShowen = true;
            }
            _ErrorText.Text = text;
        }

        private void LoginCallback(object rev)
        {
            string recived = (string)rev; 
            switch (recived[0])
            {
                case '1':
                    //Login is correct, setting up profile and redirecting to MainAfterLoginPage

                    LocalUser.Username = Input_Username.Text;
                    LocalUser.IsLoggedIn = true;
                    StaticNavigator.Push(new MainAfterLoginPage(true));
                    break;
                case '0':
                    ShowError("Password or username is incorrect");
                    break;
                default:
                    ShowError("Samething went wrong, probably on server side ... ");
                    break;
            }
        }

        private void Input_Focused(object sender, FocusEventArgs e)
        {
            ClearError();
        }
    }
}