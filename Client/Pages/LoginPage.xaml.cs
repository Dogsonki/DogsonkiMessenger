using Client.Models.Bindable;
using Client.Networking.Core;
using Client.Networking.Packets;
using Client.Pages.Helpers;
using Newtonsoft.Json;

namespace Client.Pages;

public partial class LoginPage : ContentPage
{
    public static LoginPage Current;
    public MessagePopPage message;
    
    public LoginPage(string? info = null)
    {
        InitializeComponent();

        NavigationPage.SetHasNavigationBar(this, false);

        message = new MessagePopPage(this);

        Current = this;

        if (!string.IsNullOrEmpty(info)) message.ShowInfo(info);
    }

    private void LoginFocused(object sender, FocusEventArgs e) => message.Clear(PopType.Error);

    private async void RedirectToRegister(object sender, EventArgs e) => await Navigation.PushAsync(new RegisterPage());

    private async void RedirectToForgotPassword(object sender, EventArgs e) => await Navigation.PushAsync(new PasswordForgot.ForgotPasswordEnterEmail());

    private void LoginDone(object sender, EventArgs e)
    {
        string username = Input_Username.Text;
        string password = Input_Password.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            message.ShowError("Username or password is empty");
            return;
        }

        if (!SocketCore.SendCallback(new LoginPacket(username, password, CheckRemember.IsChecked), Token.LOGIN, LoginCallback))
        {
            message.ShowError("Unable to connect to the server");
            return;
        }
    }

    public void LoginCallback(object _login)
    {
        LoginCallbackPacket login = JsonConvert.DeserializeObject<LoginCallbackPacket>((string)_login);

        switch (login.Token)
        {
            case "1":
                LocalUser.Login(login.Username, login.ID, login.Email);
                break;
            case "0":
                message.ShowError("Wrong email or password");
                break;
            case "-1":
                message.ShowError("User is banned");
                break;
            default:
                message.ShowError("Something went wrong, probably on server side ... ");
                break;
        }
    }
}