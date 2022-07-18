using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;
using Client.Pages.Helpers;

namespace Client.Pages;

public partial class LoginPage : ContentPage
{
    private static LoginPage Instance;
    private MessagePopPage message;

	public LoginPage(string info = null)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        message = new MessagePopPage(this);

        Instance = this;

        if (!string.IsNullOrEmpty(info)) message.ShowInfo(info);
    }

    private Label ErrorText = new Label()
    {
        TextColor = Color.FromRgb(255, 0, 0)
    };

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

#if DEBUG
        if(username == "a" && password == "a")
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
                LocalUser.Login(login.Username, login.ID,login.Email);
                break;
            case "0":
                message.ShowError($"Wrong email or password");
                break;
            default:
                message.ShowError("Samething went wrong, probably on server side ... ");
                break;
        }
    }
}