using Client.Pages;
using Client.Networking.Core;
using Client.Networking.Model;

namespace Client;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		MainPage = new NavigationPage(new LoginPage());

		ReadSession();
		SocketCore.Init();
	}

	private void ReadSession()
    {
#if ANDROID
        AndroidFileService wr = new AndroidFileService();
        bool SessionExist = wr.CreateFileIfNotExist("session.json");
        if (SessionExist)
        {
			string session = File.ReadAllText(AndroidFileService.GetPersonalDir("session.json"));
            Debug.Write(session);
            if (!string.IsNullOrEmpty(session))
            {
                SocketCore.Send(session,Token.SESSION_INFO);
            }
        }
#endif
    }
}
