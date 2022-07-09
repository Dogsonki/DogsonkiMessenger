using Client.Pages;
using Client.Networking.Core;
using Client.Networking.Model;
using Newtonsoft.Json;
using Client.Models.Session;

namespace Client;

public partial class App : Application
{
    private static LoginPage CurrentLoginPage;

	public App()
	{
		InitializeComponent();
        CurrentLoginPage = new LoginPage();

        MainPage = new NavigationPage(CurrentLoginPage);

        Connection.AddOnConnection(ReadSession);

		SocketCore.Init();
	}

	private static void ReadSession()
    {
#if ANDROID
        AndroidFileService wr = new AndroidFileService();
        bool SessionExist = wr.CreateFileIfNotExist("session.json");
        if (SessionExist)
        {
            string ses = File.ReadAllText(AndroidFileService.GetPersonalDir("session.json"));
			Session session = JsonConvert.DeserializeObject<Session>(ses);
            if (session != null && !string.IsNullOrEmpty(ses))
            {  
                if(session.SessionKey != null)
                {
                    SocketCore.Send(session, Token.SESSION_INFO);
                }
            }
        }
#endif
    }
}
