using Client.Networking.Core;
using Client.Pages.Helpers;
using System.Net.Mail;

namespace Client.Pages.PasswordForgot;

public partial class ForgotPasswordEnterEmail : ContentPage
{
	private MessagePopPage message;

	public ForgotPasswordEnterEmail()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
		message = new MessagePopPage(this);
    }

	private void RecoverClicked(object sender, EventArgs e)
	{
		string ProvidedEmail = Input_Email.Text;

		if(Input_Email.Text == "a")
		{
            MainThread.BeginInvokeOnMainThread(() => StaticNavigator.Push(new ForgotPasswordEnterCode()));
        }

		if (string.IsNullOrEmpty(ProvidedEmail))
		{
			message.ShowError("Email is empty");
			return;
		}

        MailAddress _tempAdr;
        if (!MailAddress.TryCreate(ProvidedEmail, out _tempAdr))
        {
            message.ShowError("Invalid email");
            return;
        }

        if (!SocketCore.SendCallback(Input_Email.Text, Token.PASSWORD_FORGOT, CheckEmailCallback))
		{
			message.ShowError("Unable to connect to server");
		}
	}

	public void CheckEmailCallback(object data)
	{
		switch (data)
		{
			case 1:
				message.ShowError("Cannot send email");
				break;
			case 2:
				message.ShowError("User with this email dose not exists");
				break;
			case 4:
				MainThread.BeginInvokeOnMainThread(() => StaticNavigator.Push(new ForgotPasswordEnterCode()));
				break;
		}
	}
}