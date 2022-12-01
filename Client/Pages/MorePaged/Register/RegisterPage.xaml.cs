using Client.Networking.Core;
using Client.Pages.Helpers;
using Client.Pages.Register;
using System.Net.Mail;
using Client.Networking.Packets;

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

    private async void RedirectToLogin(object sender, EventArgs e) => await Navigation.PushAsync(new LoginPage());

    private readonly char[] IllegalCharacters = { '$', '{', '}', '@', ':' };

    private void RegisterDone(object sender, EventArgs e)
    {
        string username = InputUsername.Text;
        string password = InputPassword.Text;
        string email = InputEmail.Text;

        int IllegalCharIndex;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            message.ShowError("Username / password or email is empty");
            return;
        }


        if ((IllegalCharIndex = username.IndexOfAny(IllegalCharacters)) > 0)
        {
            message.ShowError($"Username contains illegal character - {username[IllegalCharIndex]}");
            return;
        }

        MailAddress _temp;
        if (!MailAddress.TryCreate(email, out _temp))
        {
            message.ShowError("Invalid email");
            return;
        }

        if (!SocketCore.SendCallback(new RegisterPacket(username, password, email), Token.REGISTER, RegisterCallback, false))
        {
            message.ShowError("Unable to connect to the server");
            return;
        }
    }

    private void RegisterCallback(object rev)
    {
        int token = int.Parse((string)rev);
        switch (token)
        {
            case 2:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case 3:
                message.ShowError("User with given email already exists");
                break;
            case 4:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case 6:
                MainThread.InvokeOnMainThreadAsync(() => Navigation.PushAsync(new ConfirmEmailCode(InputEmail.Text)));
                break;
            case 7:
                message.ShowError("User with given username already exists");
                break;
            default:
                message.ShowError($"Unknown error {token}");
                break;
        }
    }
}