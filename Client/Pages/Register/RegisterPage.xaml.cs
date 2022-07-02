using Client.Models;
using Client.Networking.Core;
using Client.Networking.Model;
using Client.Pages.Register;

namespace Client.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private async void RedirectToLogin(object sender, EventArgs e) => await Navigation.PushAsync(new LoginPage());

    protected char[] IllegalCharacters = { '$', '*', '{', '}', '#', '@' };

    private Label ErrorText = new Label()
    {
        TextColor = Color.FromRgb(255, 0, 0)
    };

    private void ShowError(string error)
    {
        ErrorText.Text = error;
        if (!ErrorLevel.Children.Contains(ErrorText))
            ErrorLevel.Children.Add(ErrorText);
    }

    private void RegisterDone(object sender, EventArgs e)
    {
        string username = InputUsername.Text;
        string password = InputPassword.Text;
        string passwordRepeat = InputPasswordRepeat.Text;
        string email = InputEmail.Text;

        int IllegalCharIndex;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordRepeat))
        {
            ShowError("Username / password or email is empty");
            return;
        }

        if (password != passwordRepeat)
        {
            ShowError("Make sure your passwords match");
            return;
        }

        if ((IllegalCharIndex = username.IndexOfAny(IllegalCharacters)) > 0)
        {
            ShowError($"Username contains illegal character - {username[IllegalCharIndex]}");
            return;
        }
        if (!SocketCore.SendR(RegisterCallback, new RegisterModel(username, password,email), Token.REGISTER))
        {
            ShowError("Unable to connect to the server");
            return;
        }
    }

    private void RegisterCallback(object rev)
    {
        RToken token = Tokens.CharToRToken(rev);
        switch (token)
        {
            case RToken.ACCEPT:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new LoginPage()));
                break;
            case RToken.EMAIL_SENT:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case RToken.USER_ALREADY_EXISTS:
                ShowError("User with given email already exists");
                break;
            case RToken.CANNOT_SEND_EMAIL:
                ShowError("Cannot send code to this email");
                break;
            case RToken.EMAIL_WAITING:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case RToken.NICKNAME_TAKEN:
                ShowError("User with given username already exists");
                break;
            default:
                ShowError($"Unknown error {(int)token}");
                break;
        }
    }
}