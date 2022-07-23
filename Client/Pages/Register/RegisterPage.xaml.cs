using Client.Models;
using Client.Networking.Core;
using Client.Pages.Helpers;
using Client.Pages.Register;
using System.Net.Mail;

namespace Client.Pages;

public partial class RegisterPage : ContentPage
{
    public MessagePopPage message;

    public RegisterPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        message = new MessagePopPage(this);
    }

    private async void RedirectToLogin(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private readonly char[] IllegalCharacters = { '$', '{', '}', '@', ':' };

    private void RegisterDone(object sender, EventArgs e)
    {
        string username = InputUsername.Text;
        string password = InputPassword.Text;
        string passwordRepeat = InputPasswordRepeat.Text;
        string email = InputEmail.Text;

        int IllegalCharIndex;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)
            || string.IsNullOrEmpty(passwordRepeat))
        {
            message.ShowError("Username / password or email is empty");
            return;
        }

        if (password != passwordRepeat)
        {
            message.ShowError("Make sure your passwords match");
            return;
        }

        if ((IllegalCharIndex = username.IndexOfAny(IllegalCharacters)) > 0)
        {
            message.ShowError($"Username contains illegal character - {username[IllegalCharIndex]}");
            return;
        }

        MailAddress _tempAdr;
        if (!MailAddress.TryCreate(email, out _tempAdr))
        {
            message.ShowError("Invalid email");
            return;
        }

        if (!SocketCore.SendCallback(RegisterCallback, new RegisterModel(username, password, email), Token.REGISTER))
        {
            message.ShowError("Unable to connect to the server");
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
                message.ShowError("User with given email already exists");
                break;
            case RToken.CANNOT_SEND_EMAIL:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case RToken.EMAIL_WAITING:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case RToken.NICKNAME_TAKEN:
                message.ShowError("User with given username already exists");
                break;
            default:
                message.ShowError($"Unknown error {(int)token}");
                break;
        }
    }
}