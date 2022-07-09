using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Utility;

namespace Client.Pages;

public partial class LoginPage : ContentPage
{
    private static LoginPage Instance;

	public LoginPage(string info = null)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        Instance = this;

        if (!string.IsNullOrEmpty(info)) AddInfo(info);
    }

    private Label InfoText = new Label()
    {
        TextColor = Color.FromRgb(255, 255, 255),
        FontAttributes = FontAttributes.Bold,
        FontSize = 16,
        HorizontalTextAlignment = TextAlignment.Center
    };

    public void AddInfo(string info)
    {
        InfoText.Text = info;
        if (!InfoLevel.Contains(InfoText))
            InfoLevel.Children.Add(InfoText);
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

#if DEBUG
        if(username == "a" && password == "a")
        {
            LocalUser.Login("uwu", "2", "wo@");
        }
#endif
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Username or password is empty");
            return;
        }

        if (!SocketCore.SendCallback(LoginCallback, new LoginModel(username, password, CheckRemember.IsChecked), Token.LOGIN))
        {
            ShowError("Unable to connect to the server");
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
                ShowError($"Wrong email or password");
                break;
            default:
                ShowError("Samething went wrong, probably on server side ... ");
                break;
        }
    }
}