using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Pages.Helpers;
using Client.Utility;

namespace Client.Pages;

public partial class LoginPage : ContentPage
{
    public static LoginPage Current;
    public MessagePopPage message;
    
    public LoginPage(string info = null)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        message = new MessagePopPage(this);

        Current = this;

        if (!string.IsNullOrEmpty(info)) message.ShowInfo(info);
    }

    private void LoginFocused(object sender, FocusEventArgs e) => message.Clear(PopType.Error);

    private async void RedirectToRegister(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void RedirectToForgotPassword(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PasswordForgot.ForgotPasswordEnterEmail());
    }

    private void LoginDone(object sender, EventArgs e)
    {
        string username = Input_Username.Text;
        string password = Input_Password.Text;

#if DEBUG
        if (username == "a" && password == "a")
        {
            LocalUser.Login("uwu", "2", "wo@");
        }
#endif
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            message.ShowError("Username or password is empty");
            return;
        }

        if (!SocketCore.SendCallback(LoginCallback, new LoginModel(username, password, CheckRemember.IsChecked), Token.LOGIN))
        {
            message.ShowError("Unable to connect to the server");
            return;
        }
    }
    public void LoginCallback(object rev)
    {
        LoginCallbackModel login = Essential.ModelCast<LoginCallbackModel>(rev);
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
                message.ShowError("Samething went wrong, probably on server side ... ");
                break;
        }
    }
}