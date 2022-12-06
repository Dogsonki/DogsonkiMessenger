using Client.Networking.Core;
using Client.Networking.Models;
using Client.Pages.Helpers;
using System.Diagnostics;

namespace Client.Pages.Register;

public partial class ConfirmEmailCode : ContentPage
{
    private Stopwatch ResendCooldownTimer = new Stopwatch();
    private const int RESENDCOOLDOWN = 60;
    private const int MAX_CODE_ATTEMPS = 5;
    private int CheckAttemps = 0;
    public MessagePopPage message;

    public ConfirmEmailCode(string email)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        message = new MessagePopPage(this);
        ResendCooldownTimer.Start();
        noteEmail.Text = $"We've sent a code to {email} and type code to window below";
    }

    private void CheckCode(object sender, EventArgs e)
    {
        if (CheckAttemps == MAX_CODE_ATTEMPS)
        {
            message.ShowError("You have used all attemps. \n Please try again later.");
            return;
        }

        if(int.TryParse(CodeInput.Text, out int code))
        {
            if(CodeInput.Text.Length != 5)
            {
                message.ShowError("Code is too short");
            }
            else
            {
                SocketCore.SendCallback(code, Token.REGISTER, CodeSended, false);
            }
        }

    }

    private void CodeSended(object rev)
    {
        if(int.TryParse((string)rev, out int token))
        {

            switch (token)
            {
                case 9:
                    message.ShowError($"Wrong code, left {MAX_CODE_ATTEMPS - CheckAttemps}");
                    CheckAttemps++;
                    break;
                case 10:
                    CheckAttemps = MAX_CODE_ATTEMPS;
                    MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        RegisterPage rg = new RegisterPage();
                        rg.message.ShowError("Max attemps used. \n Please try again later!");
                        Navigation.PushAsync(rg);
                    });
                    break;
                case 0:
                    MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Navigation.PushAsync(new LoginPage("Your accout has been activated. \n You can now login to your account"));
                    });
                    break;
                default:
                    message.ShowError($"Unknown error {(int)token}");
                    break;
            }
        }
    }

    private void ResendCode(object sender, EventArgs e)
    {
        if (ResendCooldownTimer.Elapsed.Seconds >= RESENDCOOLDOWN)
        {
            SocketCore.Send("a", Token.REGISTER);
            ResendCooldownTimer.Reset();
            message.Clear();
        }
        else
        {
            message.ShowError($"You have to wait {RESENDCOOLDOWN - ResendCooldownTimer.Elapsed.Seconds} to send code again");
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