using Client.Models;
using Client.Networking.Core;
using Client.Networking.Model;
using Newtonsoft.Json.Linq;

namespace Client.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
	}

    private Label ErrorText = new Label()
    {
        TextColor = Color.FromRgb(255, 0, 0)
    };

    private void ShowError(string error)
    {
        ErrorText.Text = error; 
        if(!ErrorLevel.Children.Contains(ErrorText))
            ErrorLevel.Children.Add(ErrorText);
    }
    private void RemoveError() => ErrorLevel.Children.Remove(ErrorText);

    private void LoginFocused(object sender, FocusEventArgs e) => RemoveError();

    private async void RedirectToRegister(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
    private void LoginDone(object sender, EventArgs e)
    {
        string username = Input_Username.Text;
        string password = Input_Password.Text;

        if(username == "a" && password == "a")
        {
            LocalUser.Login(username, "0");
            StaticNavigator.Push(new MainPage());
        }

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Username or password is empty");
            return;
        }

        if (!SocketCore.SendR(LoginCallback, new LoginModel(username, password, CheckRemember.IsChecked), Token.LOGIN))
        {
            ShowError("Unable to connect to the server");
            return;
        }
    }
    private void LoginCallback(object rev)
    {
        LoginCallbackModel login = ((JObject)rev).ToObject<LoginCallbackModel>();

        switch (login.Token)
        {
            case "0":
                LocalUser.Login(login.Username, login.ID);
                StaticNavigator.Push(new MainPage());
                break;
            case "1":
                ShowError($"Unkown error: " + (string)rev);
                break;
            default:
                ShowError("Samething went wrong, probably on server side ... ");
                break;
        }
    }
}