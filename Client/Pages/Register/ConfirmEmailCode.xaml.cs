using Client.Networking.Core;
using System.Diagnostics;

namespace Client.Pages.Register;

public partial class ConfirmEmailCode : ContentPage
{
    private Stopwatch ResendCooldownTimer = new Stopwatch();
    private const int RESENDCOOLDOWN = 60;
    private const int MAX_CODE_ATTEMPS = 5;
    private int CheckAttemps = 0;

    public ConfirmEmailCode(string email)
    {
        InitializeComponent();
        CheckAttemps = 0;
        NavigationPage.SetHasNavigationBar(this, false);
        ResendCooldownTimer.Start();
        noteEmail.Text = $"We've sent a code to {email} and type code to window below";
    }

    private void CheckCode(object sender, EventArgs e)
    {
        if (CheckAttemps == MAX_CODE_ATTEMPS)
        {
            return;
        }
        SocketCore.SendCallback(CodeSended, ((Entry)sender).Text, Token.REGISTER);
    }

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
    private void RemoveError()
    {
        if (ErrorLevel.Children.Contains(ErrorText))
            ErrorLevel.Children.Remove(ErrorText);
    }

    private void CodeSended(object rev)
    {
        RToken token = Tokens.CharToRToken(rev);
        switch (token)
        {
            case RToken.WRONG_CODE:
                ShowError($"Wrong code, left {MAX_CODE_ATTEMPS - CheckAttemps}");
                CheckAttemps++;
                break;
            case RToken.MAX_CODE_ATTEMPS:
                CheckAttemps = MAX_CODE_ATTEMPS;
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    RegisterPage rg = new RegisterPage();
                    rg.message.ShowError("Max attemps used. \n Please try again later!");
                    Navigation.PushAsync(rg);
                });
                break;
            case RToken.ACCEPT:
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Navigation.PushAsync(new LoginPage("Your accout has been activated. \n You can now login to your account"));
                });
                break;
            default:
                ShowError($"Unknown error {(int)token}");
                break;
        }
    }

    private void ResendCode(object sender, EventArgs e)
    {
        if (ResendCooldownTimer.Elapsed.Seconds >= RESENDCOOLDOWN)
        {
            SocketCore.Send("a", Token.REGISTER);
            ResendCooldownTimer.Reset();
            RemoveError();
        }
        else
        {
            ShowError($"You have to wait {RESENDCOOLDOWN - ResendCooldownTimer.Elapsed.Seconds} to send code again");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (CheckAttemps < MAX_CODE_ATTEMPS)
        {
            SocketCore.Send("b", Token.REGISTER);
        }
        return base.OnBackButtonPressed();
    }
}