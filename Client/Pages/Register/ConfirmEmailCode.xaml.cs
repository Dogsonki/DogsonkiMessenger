using Client.Networking.Core;
using Client.Networking.Model;
using System.Diagnostics;

namespace Client.Pages.Register;

public partial class ConfirmEmailCode : ContentPage
{
    private Stopwatch ResendCooldownTimer = new Stopwatch();
    private const int RESENDCOOLDOWN = 15;

	public ConfirmEmailCode(string email)
	{
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this, false);
        ResendCooldownTimer.Start();
		noteEmail.Text = $"We've sent a code to {email} and type code to window below";
	}

    private void CheckCode(object sender, EventArgs e)
    {
        SocketCore.SendR(CodeSended,((Entry)sender).Text,Token.REGISTER);
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
        if(ErrorLevel.Children.Contains(ErrorText))
            ErrorLevel.Children.Remove(ErrorText);
    }

    private void CodeSended(object rev)
    {
        char token = ((string)rev)[0];
        switch (token)
        {
            case '9':
                MainThread.BeginInvokeOnMainThread(() => StaticNavigator.PopAndPush(new LoginPage()));
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
    private void Cancel(object sender, EventArgs e)
    {
        SocketCore.Send("b", Token.REGISTER);
    }
}