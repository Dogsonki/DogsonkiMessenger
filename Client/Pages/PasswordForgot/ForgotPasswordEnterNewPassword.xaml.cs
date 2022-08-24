using Client.Networking.Core;
using Client.Pages.Helpers;

namespace Client.Pages.PasswordForgot;

public partial class ForgotPasswordEnterNewPassword : ContentPage
{
	MessagePopPage message;

	public ForgotPasswordEnterNewPassword()
	{
		NavigationPage.SetHasNavigationBar(this, false);
		InitializeComponent();
		message = new MessagePopPage(this);
	}

	private void SetPassword(object sender, EventArgs e)
	{
		string password = NewPassword.Text;
		string repeatPassword = RepeatNewPassword.Text;

		if(password != repeatPassword)
		{
			message.ShowError("Passwords are not the same");
			return;
		}

		if(string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword))
		{
			message.ShowError("Password is empty");
			return;
		}

        if (!SocketCore.SendCallback<int>(CheckPasswordCallback, password, Token.PASSWORD_FORGOT))
        {
            message.ShowError("Unable to connect to server");
        }
    }

	private void CheckPasswordCallback(int data)
	{
		switch (data)
		{
			case 0:
                MainThread.InvokeOnMainThreadAsync(async () => await Navigation.PushAsync(new LoginPage("Password changed. \n You can now login in. ")));
                break;
			case 3:
				message.ShowError("Password doesn't match requirements");
				break;
;		}
	}
}