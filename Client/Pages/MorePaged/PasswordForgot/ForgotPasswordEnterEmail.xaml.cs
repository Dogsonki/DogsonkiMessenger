using Client.Networking.Core;
using Client.Networking.Models;
using Client.Pages.Helpers;
using Client.Utility;
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

        if (!SocketCore.SendCallback(Input_Email.Text, Token.PASSWORD_FORGOT, CheckEmailCallback, false))
		{
			message.ShowError("Unable to connect to server");
		}
	}

    public void CheckEmailCallback(object data)
	{
		int code = int.Parse((string)data);
        switch (code)
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