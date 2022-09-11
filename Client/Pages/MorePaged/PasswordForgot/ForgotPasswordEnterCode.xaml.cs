using Client.Networking.Core;
using Client.Pages.Helpers;

namespace Client.Pages.PasswordForgot;

public partial class ForgotPasswordEnterCode : ContentPage
{
	private MessagePopPage message;
	private const int MAX_ATTEMPTS = 5;
	private int Attempts = 0;

	public ForgotPasswordEnterCode()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
		message = new MessagePopPage(this);
    }

	private void CheckCode(object sender, EventArgs e)
	{
		message.Clear(PopType.Error);
		message.Clear(PopType.Info);

		int code;
		if(Attempts == MAX_ATTEMPTS)
		{
			MainThread.InvokeOnMainThreadAsync(async () => await Navigation.PushAsync(new LoginPage("You used all attemps. Please try again ")));
			return;
		}
		if(!int.TryParse(CodeInput.Text,out code))
		{
			message.ShowError("Code is not a number");
			return;
		}
		if(code.ToString().Length != 5)
		{
			message.ShowError("Code is too short");
			return;
		}
		if(!SocketCore.SendCallback(CheckCodeCallback, code, Token.PASSWORD_FORGOT))
		{
            message.ShowError("Unable to connect to server");
        }
	}

	private void CheckCodeCallback(object data)
	{
		switch (data)
		{
			case 9:
                Attempts++;
				message.ShowError($"Wrong verification code. Attemps: {MAX_ATTEMPTS - Attempts}");
				break;
			case 5:
				MainThread.InvokeOnMainThreadAsync(async () => await Navigation.PushAsync(new ForgotPasswordEnterNewPassword()));
				break;
		}
	}
}